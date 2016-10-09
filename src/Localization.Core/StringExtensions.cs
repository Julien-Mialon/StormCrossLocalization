using System.Linq;

namespace Localization.Core
{
	public static class StringExtensions
	{
		private static readonly string[] _platformSuffix = {Constants.WINDOWS_PHONE_SUFFIX, Constants.ANDROID_SUFFIX, Constants.IOS_SUFFIX};

		public static string ProcessKey(this string source)
		{
			return source?.Replace(".", "__");
		}

		public static string UnprocessKey(this string source)
		{
			return source?.Replace("__", ".");
		}

		public static string SimplifyKey(this string source)
		{
			if (source == null)
			{
				return null;
			}

			foreach (string suffix in _platformSuffix)
			{
				if (source.EndsWith(suffix))
				{
					return source.Substring(0, source.Length - suffix.Length);
				}
			}

			return source;
		}

		public static bool IsPlatformSpecificString(this string source)
		{
			if (source == null)
			{
				return false;
			}
			return _platformSuffix.Any(source.EndsWith);
		}

		public static bool IsAndroidString(this string source)
		{
			if (source == null)
			{
				return false;
			}
			return source.EndsWith(Constants.ANDROID_SUFFIX);
		}

		public static bool IsIosString(this string source)
		{
			if (source == null)
			{
				return false;
			}
			return source.EndsWith(Constants.IOS_SUFFIX);
		}

		public static bool IsWindowsPhoneString(this string source)
		{
			if (source == null)
			{
				return false;
			}
			return source.EndsWith(Constants.WINDOWS_PHONE_SUFFIX);
		}
	}
}
