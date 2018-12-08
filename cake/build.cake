#addin "Cake.ExtendedNuGet"
#addin "nuget:?package=NuGet.Core&version=2.12.0"


const string ROOT_PATH = "../src/";
const string SOLUTION_PATH = ROOT_PATH + "StormCrossLocalization.sln";
const string ARTIFACTS_DIRECTORY = "../artifacts";
const string DEPLOYMENT_DIRECTORY = "../build";
const string DEPLOYMENT_TOOLS_DIRECTORY = DEPLOYMENT_DIRECTORY + "/localization";
const string DEPLOYMENT_BUILD_DIRECTORY = DEPLOYMENT_DIRECTORY + "/build";
const string OUTPUT_BUILD_PATH = "/bin/Release/netstandard2.0/publish/";

const string PCL_TFM = "netstandard2.0";
const string WINPHONE_TFM = "win;wp;wp8;wp81;wpa81;uap";
const string ANDROID_TFM = "monoandroid";
const string IOS_TFM = "monotouch;xamarinios";

const string NUGET_NAME = "Storm.CrossLocalization";
const string NUGET_VERSION = "1.2.0";
const string NUGET_AUTHOR = "Julien Mialon";

/* constants for target names */
const string CLEAN = "clean";
const string FULL_CLEAN = "mrproper";
const string RESTORE = "restore";
const string BUILD = "build";
const string REBUILD = "rebuild";
const string RELEASE = "release";
const string DEPLOY = "deploy";
const string DEFAULT_TARGET = BUILD;

const string VERSION_BETA_ARG = "args.beta";

string[] LibProjects = new []
{
	"Localization.Core",
	"Localization.PCL",
	"Localization.Android",
	"Localization.iOS",
	"Localization.WindowsPhone"
};

string[] DependencyLibraries = new []
{
	"Newtonsoft.Json.dll"
};

Tuple<string, string>[] TFMForProjects = new []
{
	Tuple.Create("Localization.PCL", PCL_TFM),
	Tuple.Create("Localization.Android", ANDROID_TFM),
	Tuple.Create("Localization.iOS", IOS_TFM),
	Tuple.Create("Localization.WindowsPhone", WINPHONE_TFM)
};

/* common tasks */
Task(BUILD)
	.IsDependentOn(RESTORE)
	.Does(() => {
		foreach(string libproject in LibProjects)
		{
			DotNetBuild(ROOT_PATH + libproject + "/" + libproject + ".csproj", c => c.SetConfiguration("Release")
				.SetVerbosity(Verbosity.Minimal)
				.WithTarget("Publish")
				.WithProperty("Platform", "AnyCPU"));
		}
	});

Task(REBUILD)
	.IsDependentOn(FULL_CLEAN)
	.IsDependentOn(BUILD)
	.Does(() => {});

Task(RELEASE)
	.IsDependentOn(REBUILD)
	.Does(() => {
		CreateDirectory(DEPLOYMENT_DIRECTORY);
		CreateDirectory(DEPLOYMENT_TOOLS_DIRECTORY);
		CreateDirectory(DEPLOYMENT_BUILD_DIRECTORY);
		CreateDirectory(ARTIFACTS_DIRECTORY);

		foreach(string libproject in LibProjects)
		{
			CopyFileToDirectory(ROOT_PATH + libproject + OUTPUT_BUILD_PATH + libproject + ".dll", DEPLOYMENT_TOOLS_DIRECTORY);
		}

		foreach(string dependencyLibrary in DependencyLibraries)
		{
			//copy newtonsoft dependency
			CopyFileToDirectory(ROOT_PATH + LibProjects[0] + OUTPUT_BUILD_PATH + dependencyLibrary, DEPLOYMENT_TOOLS_DIRECTORY);
		}

		foreach(Tuple<string, string> tfmForProject in TFMForProjects)
		{
			string[] tfms = tfmForProject.Item2.Split(';');
			string projectName = tfmForProject.Item1;
			foreach(string tfm in tfms)
			{
				string directory = DEPLOYMENT_BUILD_DIRECTORY + "/" + tfm;
				CreateDirectory(directory);
				CopyFile(ROOT_PATH + projectName + "/" + projectName + ".targets", directory + "/" + NUGET_NAME + ".targets");
				CopyFile(ROOT_PATH + projectName + "/" + projectName + ".props", directory + "/" + NUGET_NAME + ".props");
			}
		}

		//generate nuspec
		string content = System.IO.File.ReadAllText(NUGET_NAME + ".nuspec");

		string betaVersion = Argument(VERSION_BETA_ARG, "");
		string fullVersion = NUGET_VERSION;
		if(!string.IsNullOrEmpty(betaVersion) && int.TryParse(betaVersion, out int betaVersionNumber))
		{
			fullVersion = $"{fullVersion}-beta-{betaVersionNumber:D5}";
		}

		content = content.Replace("{id}", NUGET_NAME)
							.Replace("{version}", fullVersion)
							.Replace("{author}", NUGET_AUTHOR);
		System.IO.File.WriteAllText(DEPLOYMENT_DIRECTORY + "/" + NUGET_NAME + ".nuspec", content);

		NuGetPack(DEPLOYMENT_DIRECTORY + "/" + NUGET_NAME + ".nuspec", new NuGetPackSettings{
			OutputDirectory = ARTIFACTS_DIRECTORY
		});
		string nugetPackagePath = ARTIFACTS_DIRECTORY + "/" + NUGET_NAME + "." + fullVersion + ".nupkg";
		string outputNugetPackagePath = ARTIFACTS_DIRECTORY + "/" + NUGET_NAME + ".nupkg";
		MoveFile(nugetPackagePath, outputNugetPackagePath);
	});

Task(DEPLOY)
	.IsDependentOn(RELEASE)
	.Does(() => {
		NuGetPush(ARTIFACTS_DIRECTORY + "/" + NUGET_NAME + ".nupkg", new NuGetPushSettings
		{
			Source = "https://www.nuget.org/api/v2/package",
			ApiKey = EnvironmentVariable("NUGET_API_KEY") ?? ""
		});
	});

/* Restore tasks */
Task(RESTORE)
	.Does(() => {
		NuGetRestore(SOLUTION_PATH);
	});

/* Cleanup tasks */
Task(CLEAN)
	.Does(() => {
		DeleteDirectories(GetDirectories(ROOT_PATH + "**/bin"), true);
		DeleteDirectories(GetDirectories(ROOT_PATH + "**/obj"), true);
	});

Task(FULL_CLEAN)
	.IsDependentOn(CLEAN)
	.Does(() => {
		if(DirectoryExists(DEPLOYMENT_DIRECTORY))
		{
			DeleteDirectory(DEPLOYMENT_DIRECTORY, true);
		}

		if(DirectoryExists(ARTIFACTS_DIRECTORY))
		{
			DeleteDirectory(ARTIFACTS_DIRECTORY, true);
		}
	});


/* System to run default task when none is specified */
Task("Default")
	.Does(() => {
		RunTarget(DEFAULT_TARGET);
	});

var target = Argument("target", DEFAULT_TARGET);
RunTarget(target);
