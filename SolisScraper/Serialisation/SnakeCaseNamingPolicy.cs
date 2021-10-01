using System.Text.Json;

namespace SolisScraper.Serialisation
{
	public class SnakeCaseNamingPolicy : JsonNamingPolicy
	{
		public static SnakeCaseNamingPolicy Instance { get; } = new();

		public override string ConvertName(string name)
		{
			return ToSnakeCase(name);
		}

		private static string ToSnakeCase(string str)
		{
			if (str == null)
			{
				return null;
			}

			var upper = 0;
			for (var i = 0; i < str.Length; i++)
			{
				if (i == 0)
				{
					continue;
				}
				var c = str[i];
				if (char.IsUpper(c))
					upper++;
			}

			//if (upper == 0)
			//	return str;

			return string.Create(str.Length + upper, str, (span, input) =>
			{
				var j = 0;
				for (var i = 0; i < input.Length; i++)
				{
					var c = input[i];
					if (i != 0 && char.IsUpper(c))
					{
						span[j++] = '_';
					}

					span[j++] = char.ToLowerInvariant(c);
				}
			});
		}
	}
}