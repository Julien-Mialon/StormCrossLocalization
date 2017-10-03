using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Localization.Core
{
	public class JsonFileReader : IFileReader
	{
		public bool HasSupportForFile(string file)
		{
			string extension = Path.GetExtension(file)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
			return extension == "json";
		}

		public Dictionary<string, string> ReadResourceFile(string file)
		{
			JObject result = JToken.Parse(File.ReadAllText(file)) as JObject;
			if (result == null)
			{
				return new Dictionary<string, string>();
			}

			return result.Properties().ToDictionary(x => x.Name, x => x.Value.ToString());
		}
	}
}