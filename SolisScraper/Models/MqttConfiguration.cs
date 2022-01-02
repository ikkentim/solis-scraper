using System;

namespace SolisScraper.Models
{
	public class MqttConfiguration
	{
		public string Host { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		
		public string ClientId { get; set; } = "solis_scraper";
		public bool DebugLogging { get; set; }
		public string DiscoveryPrefix { get; set; } = "homeassistant";
		public string NodeId { get; set; } = "solis";
		public string UniqueIdPrefix { get; set; } = "solis_scraper";
		public int FailureCap { get; set; } = 5;
		
		public TimeSpan IntervalError { get; set; } = TimeSpan.FromSeconds(30);
		public TimeSpan IntervalZero { get; set; } = TimeSpan.FromSeconds(30);
		public TimeSpan IntervalValue { get; set; } = TimeSpan.FromSeconds(15);
		public TimeSpan IntervalDuplicateState { get; set; } = TimeSpan.FromMinutes(15);
	}
}