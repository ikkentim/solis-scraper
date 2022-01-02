using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolisScraper.Models;

namespace SolisScraper
{
	public class ScraperService : BackgroundService
	{
		private readonly MqttConfiguration _configuration;
		private readonly MqttTransmitter _mqttClient;
		private readonly SolarClient _solarClient;
		private SolarScrapeResult _previousResult;
		private State _state;
		private readonly ILogger _logger;

		public ScraperService(SolarClient solarClient, MqttTransmitter mqttClient, IOptions<MqttConfiguration> options, ILogger<ScraperService> logger)
		{
			_logger = logger;
			_configuration = options.Value;
			_mqttClient = mqttClient;
			_solarClient = solarClient;
		}

		private string TopicState => $"solis_scraper/sensor/{_configuration.NodeId}/state";

		private string TopicConfig(string name) => $"{_configuration.DiscoveryPrefix}/sensor/{_configuration.NodeId}/{name}/config";

		private async Task SendEntityConfigurations()
		{
			var device = new HassDevice
			{
				Identifiers = new[]
				{
					_configuration.NodeId
				},
				Name = "Solis Energy"
			};

			await RetryUntilSuccess(() =>
				_mqttClient.Send(TopicConfig("solis-now"), new HassConfig
				{
					DeviceClass = "power",
					Name = "Solis Energy Production (now)",
					StateTopic = TopicState,
					UnitOfMeasurement = "W",
					ValueTemplate = "{{ value_json.watt_now }}",
					StateClass = "measurement",
					ForceUpdate = true,
					Icon = "mdi:solar-power",
					Device = device,
					UniqueId = $"{_configuration.UniqueIdPrefix}_now"
				})
			);

			await RetryUntilSuccess(() =>
				_mqttClient.Send(TopicConfig("solis-today"), new HassConfig
				{
					DeviceClass = "energy",
					Name = "Solis Energy Production (today)",
					StateTopic = TopicState,
					UnitOfMeasurement = "kWh",
					ValueTemplate = "{{ value_json.kilo_watt_today }}",
					StateClass = "total_increasing",
					ForceUpdate = true,
					Icon = "mdi:solar-power",
					Device = device,
					UniqueId = $"{_configuration.UniqueIdPrefix}_today"
				})
			);

			await RetryUntilSuccess(() =>
				_mqttClient.Send(TopicConfig("solis-total"), new HassConfig
				{
					DeviceClass = "energy",
					Name = "Solis Energy Production (total)",
					StateTopic = TopicState,
					UnitOfMeasurement = "kWh",
					ValueTemplate = "{{ value_json.kilo_watt_total }}",
					StateClass = "total_increasing",
					ForceUpdate = true,
					Icon = "mdi:solar-power",
					Device = device,
					UniqueId = $"{_configuration.UniqueIdPrefix}_total"
				})
			);
		}

		private void SetState(State state, string message = null)
		{
			if (_state != state)
			{
				_logger.LogInformation($"State transitioned from {_state} to {state}. {message}");
				_state = state;
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await RetryUntilSuccess(() => _mqttClient.Start());

			var didSetup = false;
			var failures = 0;
			var sleepResultSent = false;
			var lastState = DateTime.UtcNow;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// Scrape status page of remote.
					SolarScrapeResult result = null;
					try
					{
						result = await _solarClient.Scrape(stoppingToken);
						failures = 0;
					}
					catch (HttpRequestException)
					{
						// remote is offline, probably due to lack of power generation
						result = null;
					}
					catch (ResponseParseException e)
					{
						SetState(State.SolarBadReply, e.Message);

						if (failures++ < _configuration.FailureCap)
						{
							await Task.Delay(_configuration.IntervalError, stoppingToken);
							continue;
						}
					}
					catch (TaskCanceledException e)
					{
						if (e.CancellationToken == stoppingToken)
						{
							continue;
						}

						SetState(State.SolarUnavailable, e.Message);
						
						if (failures++ < _configuration.FailureCap)
						{
							await Task.Delay(_configuration.IntervalError, stoppingToken);
							continue;
						}
					}

					// When no new result could be scraped, assume the remote is sleeping due to no generation. Assume the previous result with a current watt value of 0.
					if (result == null)
					{
						if (!sleepResultSent && _previousResult != null)
						{
							result = _previousResult;
							result.WattNow = 0;

							// TODO: Reset after midnight? result.KiloWattToday

							sleepResultSent = true;
						}
						else
						{
							await Task.Delay(_configuration.IntervalZero, stoppingToken);
							continue;
						}
					}
					else
					{
						sleepResultSent = false;
					}

					// Filter duplicates to reduce state changes
					if (result.Equals(_previousResult))
					{
						// when state did not change since last loop, check when last state was sent. if it
						// was < IntervalDuplicateState ago, do not send the state during this loop.
						if (DateTime.UtcNow - lastState < _configuration.IntervalDuplicateState)
						{
							await Task.Delay(_configuration.IntervalZero, stoppingToken);
							continue;
						}
					}
					
					// Send state to mqtt.
					await _mqttClient.Send(TopicState, result, false);
					_previousResult = result;
					lastState = DateTime.UtcNow;
					
					// Send configuration of entities to mqtt after the initial state.
					if (!didSetup)
					{
						await SendEntityConfigurations();
						didSetup = true;
					}

					// Sleep for next scrape cycle.
					SetState(State.Running);
					await Task.Delay(result.WattNow > 0 ? _configuration.IntervalValue : _configuration.IntervalZero, stoppingToken);
				}
				catch (TaskCanceledException e)
				{
					if (stoppingToken.IsCancellationRequested)
					{
						return;
					}

					SetState(State.Unknown, e.Message);
					await Task.Delay(_configuration.IntervalError, stoppingToken);
				}
				catch (Exception e)
				{
					SetState(State.MqttUnavailable, e.Message);
					_logger.LogError(e, "Publishing failed");
					await Task.Delay(_configuration.IntervalError, stoppingToken);
				}
			}

			await _mqttClient.Stop();
		}

		private async Task RetryUntilSuccess(Func<Task> action)
		{
			var ok = false;
			while (!ok)
			{
				try
				{
					await action();
					ok = true;
				}
				catch (Exception e)
				{
					_logger.LogError(e, "Action failed, retrying...");
					await Task.Delay(1000);
				}
			}
		}


		private enum State
		{
			Initial,
			SolarUnavailable,
			SolarBadReply,
			MqttUnavailable,
			Running,
			Unknown
		}
	}
}