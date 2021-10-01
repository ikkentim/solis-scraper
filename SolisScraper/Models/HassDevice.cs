namespace SolisScraper.Models
{
	public class HassDevice
	{
		public string[] Identifiers { get; set; }
		public string Manufacturer { get; set; } = "Tim";
		public string Model { get; set; } = "Solis";
		public string Name { get; set; }
		public string SwVersion { get; set; } = "Tim's Solis Tool 0.0.1";
	}
}