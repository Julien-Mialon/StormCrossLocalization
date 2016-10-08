using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace Localization.Core
{
	public abstract class BaseLocalizationTask : Task
	{
		private const string LINK_METADATA_NAME = "Link";

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		[Required]
		public string DefaultNamespace { get; set; }

		[Required]
		public string Namespace { get; set; }

		[Output]
		public ITaskItem[] GeneratedStrings { get; private set; }

		[Output]
		public ITaskItem[] GeneratedCSharp { get; private set; }

		private readonly List<string> _keys = new List<string>();

		public override bool Execute()
		{
			Dictionary<string, List<ResxFile>> resxFiles = new Dictionary<string, List<ResxFile>>();

			Log.LogMessage(MessageImportance.High, "StormCrossLocalization: Processing strings file");
			BeforeRead();
			foreach (ITaskItem inputFile in InputFiles)
			{
				string absoluteFilePath = inputFile.ItemSpec;
				string projectFilePath = inputFile.GetOrDefaultMetadata(LINK_METADATA_NAME, absoluteFilePath);

				string directory = Path.GetDirectoryName(projectFilePath) ?? string.Empty;

				if (!resxFiles.ContainsKey(directory))
				{
					resxFiles.Add(directory, new List<ResxFile>());
				}
				Log.LogMessage(MessageImportance.High, $"\t- {projectFilePath}");
				resxFiles[directory].Add(new ResxFile
				{
					AbsoluteFilePath = absoluteFilePath,
					ProjectFilePath = projectFilePath,
					Directory = directory,
					Content = ReadResourceFile(absoluteFilePath)
				});
			}
			AfterRead();

			BeforeGeneration();
			foreach (string directory in resxFiles.Keys)
			{
				Generate(directory, resxFiles[directory]);
			}
			AfterGeneration();

			CompleteTask();
			return !Log.HasLoggedErrors;
		}

		protected virtual Dictionary<string, string> ReadResourceFile(string file)
		{
			string extension = Path.GetExtension(file)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
			if (extension == "resw" || extension == "resx")
			{
				XElement rootElement = XElement.Load(file);
				return (from dataElement in rootElement.Descendants("data")
						let key = dataElement.Attribute("name")?.Value ?? string.Empty
						where !string.IsNullOrEmpty(key)
						let value = dataElement.Element("value")?.Value
						where value != null
						select new { key, value }).ToDictionary(x => x.key, x => x.value);
			}
			throw new InvalidOperationException($"File {file} is not a supported format");
		}

		protected virtual void BeforeRead()
		{
			
		}

		protected virtual void AfterRead()
		{
			
		}

		protected virtual void BeforeGeneration()
		{
			
		}

		protected virtual void Generate(string directory, List<ResxFile> files)
		{
			
		}

		protected virtual void AfterGeneration()
		{
			
		}

		protected virtual void CompleteTask()
		{
			
		}
	}
}
