using System;
using Microsoft.Build.Framework;

namespace Localization.Core
{
	public static class TaskItemExtensions
	{
		public static string GetOrDefaultMetadata(this ITaskItem item, string key, string defaultValue = null)
		{
			foreach (string metadataKey in item.MetadataNames)
			{
				if (key.Equals(metadataKey, StringComparison.InvariantCultureIgnoreCase))
				{
					return item.GetMetadata(metadataKey);
				}
			}
			return defaultValue;
		}
	}
}
