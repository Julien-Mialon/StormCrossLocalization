namespace Localization.Core
{
	public static class Constants
	{
		//Platform switch
		public const string WINDOWS_PHONE_SUFFIX = "_WP";
		public const string ANDROID_SUFFIX = "_Android";
		public const string IOS_SUFFIX = "_iOS";

		//internal use
		public const string ENUM_NAME = "EnumStrings";
		public const string INTERFACE_SERVICE_NAME = "ILocalizationService";
		public const string IMPLEMENTATION_SERVICE_NAME = "LocalizationService";
		public const string SERVICE_METHOD_NAME = "Get";
		public const string LOCALIZED_STRINGS_NAME = "LocalizedStrings";

		public const string CONTEXT_FIELD_NAME = "_ctx";
		public const string CONTEXT_PARAMETER_NAME = "ctx";

		//file
		public const string FILE_SUFFIX = ".Localization.cs";

		public const string ENUM_FILE_PATH = ENUM_NAME + FILE_SUFFIX;
		public const string INTERFACE_SERVICE_FILE_PATH = INTERFACE_SERVICE_NAME + FILE_SUFFIX;
		public const string IMPLEMENTATION_SERVICE_FILE_PATH = IMPLEMENTATION_SERVICE_NAME + FILE_SUFFIX;
		public const string LOCALIZED_STRINGS_FILE_PATH = LOCALIZED_STRINGS_NAME + FILE_SUFFIX;
	}
}
