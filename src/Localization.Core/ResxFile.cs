using System.Collections.Generic;

namespace Localization.Core
{
	public class ResxFile
	{
		public string AbsoluteFilePath { get; set; }

		public string ProjectFilePath { get; set; }

		public string Directory { get; set; }

		public Dictionary<string, string> Content { get; set; }
	}
}