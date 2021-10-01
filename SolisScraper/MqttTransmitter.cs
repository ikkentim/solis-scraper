using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using SolisScraper.Models;
using SolisScraper.Serialisation;

namespace SolisScraper
{
	public class MqttTransmitter
	{
		private readonly IManagedMqttClient _client;
		private readonly IManagedMqttClientOptions _options;
		private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
		private readonly MqttConfiguration _configuration;
		private readonly JsonSerializerOptions _serializerOptions = new()
		{
			PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
			IgnoreNullValues = true
		};

		private ILogger _logger;

		public MqttTransmitter(IOptions<MqttConfiguration> options, ILogger<MqttTransmitter> logger)
		{
			_configuration = options.Value;
			_options = new ManagedMqttClientOptionsBuilder()
				.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
				.WithClientOptions(new MqttClientOptionsBuilder()
					.WithClientId(_configuration.ClientId)
					.WithTcpServer(_configuration.Host)
					.WithCredentials(_configuration.Username, _configuration.Password)
					.Build())
				.Build();
			
			_logger = logger;
			_client = new MqttFactory().CreateManagedMqttClient();
		}

		public Task Start() => _client.StartAsync(_options);

		public Task Stop() => _client.StopAsync();

		public async Task Send(string topic, object obj, bool retain = true)
		{
			// Debug logging
			if (_configuration.DebugLogging)
			{
				_logger.LogDebug($"Writing to topic: {topic} (retain = {retain}):");
				_logger.LogDebug(JsonSerializer.Serialize(obj, _serializerOptions));
			}

			// Serialize
			await using var stream = MemoryStreamManager.GetStream();
			await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
			{
				Indented = false
			});
			
			JsonSerializer.Serialize(writer, obj, _serializerOptions);

			var position = stream.Position;
			stream.Seek(0, SeekOrigin.Begin);

			// Construct message
			var message = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(stream, position)
				.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
				.WithRetainFlag(retain)
				.Build();
			
			// Publish
			var result = await _client.PublishAsync(message);

			if (result.ReasonCode != MqttClientPublishReasonCode.Success)
			{
				throw new Exception($"MQTT sending failed: {result.ReasonCode}: {result.ReasonString}");
			}
		}
	}
}