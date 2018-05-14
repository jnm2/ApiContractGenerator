#load lib.cake

var configuration = Argument("configuration", "Release");

var packDir = Directory("pub");

Task("Clean").Does(() => DefaultClean());

Task("Build")
    .IsDependentOn("Clean")
    .Does(() => MSBuild("src", DefaultMSBuildSettings().WithTarget("Build").WithRestore()));

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
    .Does(() => MSBuild("src/ApiContractGenerator.MSBuild", DefaultMSBuildSettings()
        .WithTarget("Pack")
        .WithProperty("PackageOutputPath", "\"" + MakeAbsolute(packDir) + "\"")));

RunTarget(Argument("target", "Test"));
