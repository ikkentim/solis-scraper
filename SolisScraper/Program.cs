using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SolisScraper.Models;

namespace SolisScraper
{
	class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				CreateHostBuilder(args).Build().Run();
			}
			catch (OptionsValidationException e)
			{
				Console.Error.WriteLine(e.Message);
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
#if DEBUG
				.UseEnvironment("Debug")
#endif
				.ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
				.ConfigureServices((ctx, services) =>
				{
					static string Message(string where, string what) =>
						$"Missing {where.ToLowerInvariant()} {what.ToLowerInvariant()}. Configure using {where}.{what} in appsettings.json or using the {where}__{what} environment variable.";

					services.AddOptions<ScraperConfiguration>()
						.Bind(ctx.Configuration.GetSection("Scraper"))
						.Validate(v => !string.IsNullOrEmpty(v.Host), Message("Scraper", "Host"))
						.Validate(v => !string.IsNullOrEmpty(v.Username), Message("Scraper", "Username"))
						.Validate(v => !string.IsNullOrEmpty(v.Password), Message("Scraper", "Password"))
						;

					services.AddOptions<MqttConfiguration>()
						.Bind(ctx.Configuration.GetSection("Mqtt"))
						.Validate(v => !string.IsNullOrEmpty(v.Host), Message("Mqtt", "Host"))
						.Validate(v => !string.IsNullOrEmpty(v.Username), Message("Mqtt", "Username"))
						.Validate(v => !string.IsNullOrEmpty(v.Password), Message("Mqtt", "Password"))
						.Validate(v => !string.IsNullOrEmpty(v.ClientId), Message("Mqtt", "ClientId"))
						.Validate(v => !string.IsNullOrEmpty(v.DiscoveryPrefix), Message("Mqtt", "DiscoveryPrefix"))
						.Validate(v => !string.IsNullOrEmpty(v.NodeId), Message("Mqtt", "NodeId"))
						.Validate(v => !string.IsNullOrEmpty(v.UniqueIdPrefix), Message("Mqtt", "UniqueIdPrefix"))
						;
					
					services.AddTransient<SolarClient>();
					services.AddTransient<MqttTransmitter>();
					services.AddHostedService<ScraperService>();
				});


	}
}
