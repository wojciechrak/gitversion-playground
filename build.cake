#tool "nuget:?package=GitVersion.CommandLine"
// ARGUMENTS

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// PREPARATION

// Define directories.
var buildDir = Directory("./source/Foo/bin");
var slnFile = "./source/Foo.sln";
var nugetPackagesDir = "./nuget";

// TASKS

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(slnFile);
});

Task("Version")
    .Does(() => {
        var version = GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo=true,
            OutputType=GitVersionOutput.Json
        });
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(slnFile, settings =>
        settings.SetConfiguration(configuration));
});

Task("NuGet-Pack")
    .Does(() => {
        NuGetPack("./source/Foo/Foo.csproj", new NuGetPackSettings{
            OutputDirectory = nugetPackagesDir,
            Properties = new Dictionary<string,string> {
                {"Configuration", configuration}
            }
        });
    });

// TASK TARGETS

Task("Default")
    .IsDependentOn("Version")
    .IsDependentOn("Build")
    .IsDependentOn("NuGet-Pack");

// EXECUTION

RunTarget(target);
