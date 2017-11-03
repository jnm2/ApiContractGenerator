var configuration = Argument("configuration", "Release");

var packDir = Directory("pub");

Task("Restore")
    .Does(() => MSBuild("src", settings => settings.SetConfiguration(configuration).WithTarget("Restore")));

Task("Clean")
    .IsDependentOn("Restore")
    .Does(() => MSBuild("src", settings => settings.SetConfiguration(configuration).WithTarget("Clean")));

Task("Build")
    .IsDependentOn("Clean")
    .Does(() => MSBuild("src", settings => settings.SetConfiguration(configuration).WithTarget("Build")));

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach (var testProject in GetFiles("src/**/*.Tests.csproj"))
            DotNetCoreTest(testProject.FullPath, new DotNetCoreTestSettings
            {
                Configuration = configuration,
                NoBuild = true
            });
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() => MSBuild("src/ApiContractGenerator.MSBuild", settings => settings.SetConfiguration(configuration)
        .WithTarget("Pack")
        .WithProperty("PackageOutputPath", "\"" + MakeAbsolute(packDir) + "\"")));

RunTarget(Argument("target", "Test"));
