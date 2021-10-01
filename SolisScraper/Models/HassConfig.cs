namespace SolisScraper.Models
{
	public class HassConfig
	{
		public string DeviceClass { get; set; }
		public string Name { get; set; }
		public string StateTopic { get; set; }
		public string UnitOfMeasurement { get; set; }
		public string ValueTemplate { get; set; }
		public string StateClass { get; set; }
		public bool? ForceUpdate { get; set; }
		public string Icon { get; set; }
		public HassDevice Device { get; set; }
		public string UniqueId { get; set; }
	}
}