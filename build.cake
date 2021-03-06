#addin "Cake.Incubator"
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0-beta0012"
// ARGUMENTS

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// PREPARATION

// Define directories.
var buildDir = Directory("./source/Foo/bin");
var slnFile = "./source/Foo.sln";
var nugetPackagesDir = Directory("./nuget");

GitVersion version = null;
// TASKS

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    NuGetRestore(slnFile);
});

Task("Version")
    .Does(() => {
        version = GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo=true,
            OutputType=GitVersionOutput.Json
        });

	if(AppVeyor.IsRunningOnAppVeyor)
	{
	    AppVeyor.UpdateBuildVersion(version.NuGetVersionV2);
	}
        Information(version.Dump());
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => {
        MSBuild(slnFile, settings => settings.SetConfiguration(configuration));
    });

Task("NuGet-Pack")
    .IsDependentOn("Version")
    .Does(() => {
        NuGetPack("./source/Foo/Foo.csproj", new NuGetPackSettings{
            Version = version.NuGetVersionV2,
            OutputDirectory = nugetPackagesDir,
            Properties = new Dictionary<string,string> {
                {"Configuration", configuration}
            }
        });
    });

Task("Artifacts")
    .Does(() => {
        if(AppVeyor.IsRunningOnAppVeyor)
        {
            AppVeyor.UploadArtifact(nugetPackagesDir + File("Foo." + version.NuGetVersionV2 + ".nupkg"));
        }
        else
        {
            Console.WriteLine("Not running on AppVeyor, skipping artifacts upload.");
        }
    });

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("NuGet-Pack")
    .IsDependentOn("Artifacts");

// EXECUTION

RunTarget(target);
