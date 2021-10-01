using System.Collections.Generic;

namespace SolisScraper.Models
{
	public class SolarScrapeResult
	{
		public decimal WattNow { get; set; }
		public decimal KiloWattToday { get; set; }
		public decimal KiloWattTotal { get; set; }

		public Dictionary<string, string> Attributes { get; set; }

		public override string ToString()
		{
			return $"{{{WattNow}W, {KiloWattToday}kWh/day, {KiloWattTotal}KWh/total}}";
		}
	}
}