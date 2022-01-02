using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolisScraper.Models;

namespace SolisScraper
{
	public class SolarClient
	{
		private readonly HttpClient _httpClient;
		
		private static readonly Regex VarRegex = new("var ([a-zA-Z_]+) = \"(.*)\";");

		public SolarClient(IOptions<ScraperConfiguration> options)
		{
			var scraperConfiguration = options.Value;

			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(scraperConfiguration.Host),
				Timeout = scraperConfiguration.Timeout,
			};

			var authString = $"{scraperConfiguration.Username}:{scraperConfiguration.Password}";
			var authBytes = Encoding.ASCII.GetBytes(authString);
			var authToken = Convert.ToBase64String(authBytes);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
		}

		public async Task<SolarScrapeResult> Scrape(CancellationToken token)
		{
			var response = await _httpClient.GetAsync("status.html", token);

			var body = await response.Content.ReadAsStringAsync(token);

			var matches = VarRegex.Matches(body);

			var dict = matches.ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

			return new SolarScrapeResult
			{
				WattNow = Parse("webdata_now_p", dict),
				KiloWattToday = Parse("webdata_today_e", dict),
				KiloWattTotal = Parse("webdata_total_e", dict)
			};
		}

		private static decimal Parse(string key, Dictionary<string, string> dict)
		{
			if (!dict.TryGetValue(key, out var value))
				throw new ResponseParseException($"Key '{key}' is missing is in scraped data.");

			if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed))
				throw new ResponseParseException($"Could not parse result '{value}' for key '{key}'.");

			return parsed;
		}
	}
}