#load nuget:?package=Cake.Storm.Fluent
#load nuget:?package=Cake.Storm.Fluent.Transformations
#load nuget:?package=Cake.Storm.Fluent.NuGet
#load nuget:?package=Cake.Storm.Fluent.DotNetCore

const string NUGET_VERSION = "17.0.0";

string betaVersion = Argument("args-beta", "");
string MODULE_VERSION = NUGET_VERSION;
if(!string.IsNullOrEmpty(betaVersion) && int.TryParse(betaVersion, out int betaVersionNumber))
{
    MODULE_VERSION = $"{MODULE_VERSION}-beta-{betaVersionNumber:D5}";
}

const string PCL_TFM = "netstandard2.0";
const string WINPHONE_TFM = "win;wp;wp8;wp81;wpa81;uap";
const string ANDROID_TFM = "monoandroid;net6.0-android;net7.0-android;net8.0-android";
const string IOS_TFM = "monotouch;xamarinios;net6.0-ios;net7.0-ios;net8.0-ios";

void AddToNugetPack(INugetPackConfiguration configuration, string tfms, string directory) {
    foreach(string value in tfms.Split(';')) {
        string tfm = value;
        if(tfm.StartsWith("net6.0") || tfm.StartsWith("net7.0") || tfm.StartsWith("net8.0")) {
            tfm = tfm + "1.0";
        }

        configuration.AddFile($"scripts/{directory}/Storm.CrossLocalization.targets", $"build/{tfm}");
        configuration.AddFile("scripts/Storm.CrossLocalization.props", $"build/{tfm}");
        configuration.AddFile("scripts/_._", $"lib/{tfm}");
    }
}

Configure()
	.UseRootDirectory("..")
	.UseBuildDirectory("build")
	.UseArtifactsDirectory("artifacts")
	.AddConfiguration(configuration => configuration
		.WithSolution("src/StormCrossLocalization.sln")
        .WithTargetFrameworks("netstandard2.0")
		.WithBuildParameter("Configuration", "Release")
		.WithBuildParameter("Platform", "Any CPU")
        .WithBuildParameter("ToolVersion", "VS2022")
		.UseDefaultTooling()
		.UseDotNetCoreTooling()
        .WithDotNetCoreOutputType(OutputType.Copy)
        .WithVersion(MODULE_VERSION)
	)
	//platforms configuration
	.AddPlatform("dotnet")
	//targets configuration
	.AddTarget("pack", configuration => configuration
        .UseCsprojTransformation(transformations => transformations.UpdatePackageVersionFromParameter())
        .UseNugetPack(nugetConfiguration => { 
            nugetConfiguration
                .WithAuthor("Julien Mialon")
                .WithPackageId("Storm.CrossLocalization")
                .WithVersion(MODULE_VERSION)
                .WithNuspec("cake/Storm.CrossLocalization.nuspec")
                .AddAllFilesFromArtifacts("localization");

            AddToNugetPack(nugetConfiguration, PCL_TFM, "pcl");
            AddToNugetPack(nugetConfiguration, WINPHONE_TFM, "windows");
            AddToNugetPack(nugetConfiguration, ANDROID_TFM, "android");
            AddToNugetPack(nugetConfiguration, IOS_TFM, "ios");
        })
	)
    .AddTarget("push", configuration => configuration
        .UseCsprojTransformation(transformations => transformations.UpdatePackageVersionFromParameter())
        .UseNugetPack(nugetConfiguration => nugetConfiguration
            .WithAuthor("Julien Mialon")
            .WithPackageId("Storm.CrossLocalization")
            .WithVersion(MODULE_VERSION)
            .WithNuspec("cake/Storm.CrossLocalization.nuspec")
            .AddAllFilesFromArtifacts("localization")
        )
        .UseNugetPush(pushConfiguration => pushConfiguration.WithApiKeyFromEnvironment())
    )
    //applications configuration
	.AddApplication("crosslocalization", configuration => configuration
        .WithProjects(
            "src/Localization.Core/Localization.Core.csproj",
            "src/Localization.Android/Localization.Android.csproj",
            "src/Localization.iOS/Localization.iOS.csproj",
            "src/Localization.PCL/Localization.PCL.csproj",
            "src/Localization.WindowsPhone/Localization.WindowsPhone.csproj"
        )
    )
	.Build();

RunTarget(Argument("target", "help"));