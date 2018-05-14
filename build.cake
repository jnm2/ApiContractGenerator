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
        var runner = DefaultTestRunner(configuration);
        AddAltCover(runner);
        runner.Run();
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() => MSBuild("src/ApiContractGenerator.MSBuild", DefaultMSBuildSettings()
        .WithTarget("Pack")
        .WithProperty("PackageOutputPath", "\"" + MakeAbsolute(packDir) + "\"")));

RunTarget(Argument("target", "Test"));
