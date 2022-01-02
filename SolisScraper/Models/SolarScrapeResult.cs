using System;
using System.Collections.Generic;

namespace SolisScraper.Models
{
	public class SolarScrapeResult
	{
		public decimal WattNow { get; set; }
		public decimal KiloWattToday { get; set; }
		public decimal KiloWattTotal { get; set; }

		protected bool Equals(SolarScrapeResult other)
		{
			return WattNow == other.WattNow && KiloWattToday == other.KiloWattToday && KiloWattTotal == other.KiloWattTotal;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SolarScrapeResult) obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(WattNow, KiloWattToday, KiloWattTotal);
		}

		public override string ToString()
		{
			return $"{{{WattNow}W, {KiloWattToday}kWh/day, {KiloWattTotal}KWh/total}}";
		}
	}
}