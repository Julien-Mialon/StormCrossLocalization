using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Localization.Core
{
	public class ResxFileReader : IFileReader
	{
		public bool HasSupportForFile(string file)
		{
			string extension = Path.GetExtension(file)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
			return extension == "resw" || extension == "resx";
		}

		public Dictionary<string, string> ReadResourceFile(string file)
		{
			XElement rootElement = XElement.Load(file);
			return (from dataElement in rootElement.Descendants("data")
				let key = dataElement.Attribute("name")?.Value ?? string.Empty
				where !string.IsNullOrEmpty(key)
				let value = dataElement.Element("value")?.Value
				where value != null
				select new { key, value }).ToDictionary(x => x.key, x => x.value);
		}
	}
}