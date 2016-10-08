namespace Localization.Core
{
	public static class StringExtensions
	{
		public static string ProcessKey(this string source)
		{
			return source?.Replace(".", "__");
		}
	}
}
