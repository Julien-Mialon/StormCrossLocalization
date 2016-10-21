#addin "Cake.ExtendedNuGet"
#addin "nuget:?package=NuGet.Core&version=2.12.0"


const string ROOT_PATH = "../src/";
const string SOLUTION_PATH = ROOT_PATH + "StormCrossLocalization.sln";
const string ARTIFACTS_DIRECTORY = "../artifacts";
const string DEPLOYMENT_DIRECTORY = "../build";
const string DEPLOYMENT_TOOLS_DIRECTORY = DEPLOYMENT_DIRECTORY + "/tools";
const string DEPLOYMENT_BUILD_DIRECTORY = DEPLOYMENT_DIRECTORY + "/build";

const string NUGET_NAME = "Storm.CrossLocalization";
const string NUGET_VERSION = "0.0.7";
const string NUGET_AUTHOR = "Julien Mialon";

/* constants for target names */
const string CLEAN = "clean";
const string FULL_CLEAN = "mrproper";
const string RESTORE = "restore";
const string BUILD = "build";
const string REBUILD = "rebuild";
const string RELEASE = "release";
const string DEFAULT_TARGET = BUILD;

string[] LibProjects = new []
{
	"Localization.Core",
	"Localization.PCL",
	"Localization.Android",
	"Localization.iOS",
	"Localization.WindowsPhone"
};

var target = Argument("target", DEFAULT_TARGET);

/* common tasks */
Task(BUILD)
	.IsDependentOn(RESTORE)
	.Does(() => {
		foreach(string libproject in LibProjects)
		{
			MSBuild(ROOT_PATH + libproject + "/" + libproject + ".csproj", new MSBuildSettings{
				Verbosity = Verbosity.Minimal,
				ToolVersion = MSBuildToolVersion.VS2015,
				Configuration = "Release",
				PlatformTarget = PlatformTarget.MSIL //AnyCPU
			});
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
			CopyFileToDirectory(ROOT_PATH + libproject + "/bin/Release/" + libproject + ".dll", DEPLOYMENT_TOOLS_DIRECTORY);
		}

		CopyFiles(ROOT_PATH + "**/*.targets", DEPLOYMENT_BUILD_DIRECTORY);
		CopyFileToDirectory("Storm.CrossLocalization.targets", DEPLOYMENT_BUILD_DIRECTORY);
		DeleteFiles(DEPLOYMENT_BUILD_DIRECTORY + "/**/*.Debug.targets");

		//generate nuspec
		string content = System.IO.File.ReadAllText(NUGET_NAME + ".nuspec");
		content = content.Replace("{id}", NUGET_NAME)
							.Replace("{version}", NUGET_VERSION)
							.Replace("{author}", NUGET_AUTHOR);
		System.IO.File.WriteAllText(DEPLOYMENT_DIRECTORY + "/" + NUGET_NAME + ".nuspec", content);

		NuGetPack(DEPLOYMENT_DIRECTORY + "/" + NUGET_NAME + ".nuspec", new NuGetPackSettings{
			OutputDirectory = ARTIFACTS_DIRECTORY
		});

		NuGetPush(ARTIFACTS_DIRECTORY + "/" + NUGET_NAME + "." + NUGET_VERSION + ".nupkg", new NuGetPushSettings
		{
			Source = "https://www.nuget.org/api/v2/package",
			ApiKey = EnvironmentVariable("NUGET_API_KEY") ?? ""
		});
	});

/* Restore tasks */
Task(RESTORE)
	.Does(() => {
		//NuGetRestore(SOLUTION_PATH);
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

RunTarget(target);