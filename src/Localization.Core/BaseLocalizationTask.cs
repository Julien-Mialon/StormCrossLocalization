﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
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
		
		public string Namespace { get; set; }
		
		protected string GenerationNamespace
		{
			get
			{
				if (!string.IsNullOrEmpty(Namespace))
				{
					return Namespace;
				}

				if (string.IsNullOrEmpty(DefaultNamespace))
				{
					return "Storm.Localization";
				}

				int index = DefaultNamespace.IndexOf('.');
				if (index < 0)
				{
					return DefaultNamespace;
				}

				return DefaultNamespace.Substring(0, index);
			}
		}

		public override bool Execute()
		{
			Dictionary<string, List<ResxFile>> resxFiles = new Dictionary<string, List<ResxFile>>();

			if(InputFiles == null || InputFiles.Length == 0)
			{
				return true;
			}

			Log.LogMessage(MessageImportance.High, $"StormCrossLocalization: Processing strings file ({this.GetType().Name})");
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
			Generate(resxFiles);
			AfterGeneration();

			SetOutputVariables();
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

		protected virtual void Generate(Dictionary<string, List<ResxFile>> files)
		{
			
		}

		protected virtual void AfterGeneration()
		{
			
		}

		protected virtual void SetOutputVariables()
		{
			
		}
	}
}
