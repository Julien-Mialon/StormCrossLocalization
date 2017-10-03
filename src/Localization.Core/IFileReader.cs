using System.Collections.Generic;

namespace Localization.Core
{
	public interface IFileReader
	{
		bool HasSupportForFile(string file);

		Dictionary<string, string> ReadResourceFile(string file);
	}
}