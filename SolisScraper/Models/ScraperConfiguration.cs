using System;

namespace SolisScraper.Models
{
	public class ScraperConfiguration
	{
		public string Host { get; set; }
		public string Username { get; set; } = "admin";
		public string Password { get; set; } = "admin";
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
	}
}