#load lib.cake
#load script.cake

var builder = CreateScriptBuilder();
builder.AddDotNetCoreTest();
builder.AddAltCover();
if (EnvironmentVariable("CI") != null) builder.AddCodecov();
var script = builder.Build();

var packDir = Directory("pub");

Task("Clean").Does(() => DefaultClean());

Task("Build")
    .IsDependentOn("Clean")
    .Does(() => MSBuild("src", script.DefaultMSBuildSettings().WithTarget("Build").WithRestore()));

Task("Test")
    .IsDependentOn("Build")
    .Does(() => script.Test());

Task("Pack")
    .IsDependentOn("Test")
    .Does(() => MSBuild("src/ApiContractGenerator.MSBuild", script.DefaultMSBuildSettings()
        .WithTarget("Pack")
        .WithProperty("PackageOutputPath", "\"" + MakeAbsolute(packDir) + "\"")));

RunTarget(Argument("target", "Test"));
