
const string ROOT_PATH = "../src/";
const string SOLUTION_PATH = ROOT_PATH + "StormCrossLocalization.sln";
const string DEPLOYMENT_DIRECTORY = "../artifacts";
const string DEPLOYMENT_TOOLS_DIRECTORY = DEPLOYMENT_DIRECTORY + "/tools";
const string DEPLOYMENT_BUILD_DIRECTORY = DEPLOYMENT_DIRECTORY + "/build";


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

		foreach(string libproject in LibProjects)
		{
			CopyFileToDirectory(ROOT_PATH + libproject + "/bin/Release/" + libproject + ".dll", DEPLOYMENT_TOOLS_DIRECTORY);
		}

		CopyFiles(ROOT_PATH + "**/*.targets", DEPLOYMENT_BUILD_DIRECTORY);
		DeleteFiles(DEPLOYMENT_BUILD_DIRECTORY + "/**/*.Debug.targets");
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
	});


/* System to run default task when none is specified */
Task("Default")
	.Does(() => {
		RunTarget(DEFAULT_TARGET);
	});

RunTarget(target);