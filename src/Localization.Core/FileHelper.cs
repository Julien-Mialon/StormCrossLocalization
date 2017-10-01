using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Localization.Core
{
	public static class FileHelper
	{
		public static void WriteIfDifferent(string file, string content)
		{
			if (File.Exists(file))
			{
				using (StreamReader reader = new StreamReader(file))
				{
					string actualContent = reader.ReadToEnd();
					if (actualContent == content)
					{
						return;
					}
				}
				File.Delete(file);
			}

			using (StreamWriter writer = new StreamWriter(File.OpenWrite(file)))
			{
				writer.Write(content);
			}
		}
		
		public static void WriteIfDifferent(string file, IEnumerable<string> contentLines)
		{
			WriteIfDifferent(file, string.Join(Environment.NewLine, contentLines));
		}

		public static void SaveIfDifferent(this XmlDocument document, string file)
		{
			string content = document.WriteToString();
			WriteIfDifferent(file, content);
		}

		public static string WriteToString(this XmlDocument document)
		{
			string result;
			using (MemoryStream outputStream = new MemoryStream())
			{
				using (XmlWriter writer = XmlWriter.Create(outputStream, new XmlWriterSettings
				{
					OmitXmlDeclaration = false,
					ConformanceLevel = ConformanceLevel.Document,
					Encoding = Encoding.UTF8,
					Indent = true,
					IndentChars = "\t"
				}))
				{
					document.Save(writer);
				}
				result = Encoding.UTF8.GetString(outputStream.ToArray());
			}
			
			string bomMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
			if (result.StartsWith(bomMarkUtf8))
			{
				result = result.Remove(0, bomMarkUtf8.Length);
			}
			return result.Replace("\0", "");
		}
	}
}