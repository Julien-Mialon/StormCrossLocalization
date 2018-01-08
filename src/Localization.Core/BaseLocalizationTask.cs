using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.SqlServer.Server;

namespace Localization.Core
{
	public abstract class BaseLocalizationTask : Task
	{
		public static List<IFileReader> Readers = new List<IFileReader>
		{
			new ResxFileReader(),
			new JsonFileReader()
		};
		
		private const string LINK_METADATA_NAME = "Link";

		protected readonly List<string> OutputCompileFilePath = new List<string>();
		protected readonly List<string> OutputResourceFilePath = new List<string>();

		[Required]
		public ITaskItem[] InputFiles { get; set; }

		[Output]
		public ITaskItem[] OutputCompileFiles { get; set; }

		[Output]
		public ITaskItem[] OutputResourceFiles { get; set; }

		[Required]
		public string DefaultNamespace { get; set; }
		
		public string Namespace { get; set; }
		
		public ITaskItem[] OverrideFiles { get; set; }
		
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

			if(InputFiles == null || InputFiles.Length == 0)
			{
				return true;
			}

			Log.LogMessage(MessageImportance.High, $"StormCrossLocalization: Processing strings file ({this.GetType().Name})");
			BeforeRead();
			Dictionary<string, List<ResxFile>> resxFiles = ReadInputFiles(InputFiles);
			Dictionary<string, List<ResxFile>> overrideFiles = ReadInputFiles(OverrideFiles);
			AfterRead();

			BeforeGeneration();
			Generate(resxFiles, overrideFiles);
			AfterGeneration();

			SetOutputVariables();

			OutputCompileFiles = OutputCompileFilePath.Select(x => (ITaskItem) new TaskItem(x)).ToArray();
			OutputResourceFiles = OutputResourceFilePath.Select(x => (ITaskItem)new TaskItem(x)).ToArray();

			return !Log.HasLoggedErrors;
		}

		private Dictionary<string, string> ReadResourceFile(string file)
		{
			IFileReader reader = Readers.FirstOrDefault(x => x.HasSupportForFile(file));
			if (reader == null)
			{
				throw new InvalidOperationException($"File {file} is not a supported format");
			}

			return reader.ReadResourceFile(file);
		}

		private Dictionary<string, List<ResxFile>> ReadInputFiles(ITaskItem[] inputs)
		{
			Dictionary<string, List<ResxFile>> result = new Dictionary<string, List<ResxFile>>();
			if(inputs == null)
			{
				return result;
			}

			foreach (ITaskItem inputFile in inputs)
			{
				string absoluteFilePath = inputFile.ItemSpec;
				string projectFilePath = inputFile.GetOrDefaultMetadata(LINK_METADATA_NAME, absoluteFilePath);

				string directory = Path.GetDirectoryName(projectFilePath) ?? string.Empty;

				if (!result.ContainsKey(directory))
				{
					result.Add(directory, new List<ResxFile>());
				}
				Log.LogMessage(MessageImportance.High, $"\t- {projectFilePath}");
				result[directory].Add(new ResxFile
				{
					AbsoluteFilePath = absoluteFilePath,
					ProjectFilePath = projectFilePath,
					Directory = directory,
					Content = ReadResourceFile(absoluteFilePath)
				});
			}

			return result;
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

		protected virtual void Generate(Dictionary<string, List<ResxFile>> files, Dictionary<string, List<ResxFile>> overrideFiles)
		{
			HashSet<string> keySet = new HashSet<string>();

			foreach (KeyValuePair<string, List<ResxFile>> item in files)
			{
				if (!overrideFiles.TryGetValue(item.Key, out List<ResxFile> overrideFile))
				{
					overrideFile = new List<ResxFile>();
				}
				Dictionary<string, string> strings = GenerateStrings(item.Key, item.Value, overrideFile, keySet);
				GenerateForDirectory(item.Key, strings);
			}

			List<string> keys = keySet.ToList();
			GenerateForProject(keys);
		}

		protected virtual Dictionary<string, string> GenerateStrings(string directory, List<ResxFile> inputFiles, List<ResxFile> overrideFiles, HashSet<string> keys)
		{
			Dictionary<string, string> content = new Dictionary<string, string>();
			Dictionary<string, string> platformSpecificContent = new Dictionary<string, string>();

			foreach (var file in inputFiles)
			{
				foreach (var item in file.Content)
				{
					if (item.Key.IsPlatformSpecificString())
					{
						string simplifiedKey = item.Key.SimplifyKey();
						if (IsCurrentPlatformKey(item.Key))
						{
							if (!platformSpecificContent.TryAdd(simplifiedKey, ProcessValue(item.Value)))
							{
								Log.LogError($"Duplicated key (key: {item.Key}, file: {file.AbsoluteFilePath})");
							}
						}

						keys.Add(simplifiedKey);
					}
					else
					{
						if (!content.TryAdd(item.Key, ProcessValue(item.Value)))
						{
							Log.LogError($"Duplicated key (key: {item.Key}, file: {file.AbsoluteFilePath})");
						}

						keys.Add(item.Key);
					}
				}
			}

			foreach (var file in overrideFiles)
			{
				foreach (var item in file.Content)
				{
					if (item.Key.IsPlatformSpecificString())
					{
						string simplifiedKey = item.Key.SimplifyKey();
						if (keys.Add(simplifiedKey))
						{
							Log.LogError($"Can not add new key using override file (key: {item.Key}, file: {file.AbsoluteFilePath})");
						}

						if (IsCurrentPlatformKey(item.Key))
						{
							platformSpecificContent[simplifiedKey] = ProcessValue(item.Value);
						}
					}
					else
					{
						if (keys.Add(item.Key))
						{
							Log.LogError($"Can not add new key using override file (key: {item.Key}, file: {file.AbsoluteFilePath})");
						}

						content[item.Key] = ProcessValue(item.Value);
					}
				}
			}

			foreach (var item in platformSpecificContent)
			{
				string key = item.Key;
				if (content.ContainsKey(key))
				{
					content[key] = item.Value;
				}
				else
				{
					content.Add(key, item.Value);
				}
			}

			return content;
		}

		protected virtual void GenerateForDirectory(string directory, Dictionary<string, string> keyValues)
		{
			
		}

		protected virtual void GenerateForProject(List<string> keys)
		{
			
		}

		protected virtual void AfterGeneration()
		{
			
		}

		protected virtual void SetOutputVariables()
		{
			
		}

		protected virtual string ProcessValue(string value)
		{
			return value;
		}

		protected virtual bool IsCurrentPlatformKey(string key)
		{
			return false;
		}
	}
}
