namespace Localization.Core
{
	public static class StringExtensions
	{
		public static string ProcessKey(this string source)
		{
			return source?.Replace(".", "__");
		}

		public static string SimplifyKey(this string source)
		{
			if (source == null)
			{
				return null;
			}

			foreach (string suffix in new [] {Constants.WINDOWS_PHONE_SUFFIX,Constants.ANDROID_SUFFIX,Constants.IOS_SUFFIX})
			{
				if (source.EndsWith(suffix))
				{
					return source.Substring(0, source.Length - suffix.Length);
				}
			}

			return source;
		}
	}
}
