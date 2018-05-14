MSBuildSettings DefaultMSBuildSettings()
{
    return new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal);
}

public void DefaultClean()
{
    var binDirectories = GetFiles("**/*.*proj")
        .Select(csproj => csproj.GetDirectory().Combine("bin"))
        .Where(binDirectory => DirectoryExists(binDirectory))
        .ToList();

    if (binDirectories.Any())
    {
        Information("Deleting bin directories:");

        foreach (var binDirectory in binDirectories)
        {
            for (var attempt = 1;; attempt++)
            {
                Information(binDirectory);
                try
                {
                    DeleteDirectory(binDirectory, new DeleteDirectorySettings { Recursive = true });
                    break;
                }
                catch (IOException ex) when (attempt < 3 && (WinErrorCode)ex.HResult == WinErrorCode.DirNotEmpty)
                {
                    Information("Another process added files to the directory while its contents were being deleted. Retrying...");
                }
            }
        }
    }
    else
    {
        Information("No bin directories to delete.");
    }
}

private enum WinErrorCode : ushort
{
    DirNotEmpty = 145
}

ProjectRunner DefaultTestRunner(string configuration)
{
    return new ProjectRunner(Context, "src/**/*.Tests.*proj", projectPath =>
    {
        DotNetCoreTest(projectPath.FullPath, new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true
        });
    }, configuration);
}

public void AddAltCover(ProjectRunner projectRunner)
{
    #tool AltCover
    var altcoverPath = Context.Tools.Resolve("AltCover.exe");

    // https://github.com/SteveGilham/altcover/wiki/Modes-of-Operation#instrument-now-test-later-collect-coverage-after-that

    projectRunner.WrapEachTarget(
        before: target =>
        {
            StartProcess(altcoverPath, new ProcessSettings
            {
                Arguments = $"--save --inplace --opencover --assemblyExcludeFilter=\"{target.GetFilenameWithoutExtension()}\"",
                WorkingDirectory = target.GetDirectory()
            });
        },
        after: target =>
        {
            StartProcess(altcoverPath, new ProcessSettings
            {
                Arguments = $"runner --collect --recorderDirectory=.",
                WorkingDirectory = target.GetDirectory()
            });
        });
}

public sealed class ProjectRunner
{
    private readonly ICakeContext context;
    private readonly string configuration;
    private readonly string projectFileGlob;
    private readonly Action<FilePath> projectAction;
    private readonly List<Action<FilePath>> beforeTargetActions = new List<Action<FilePath>>();
    private readonly List<Action<FilePath>> afterTargetActions = new List<Action<FilePath>>();


    public void WrapEachTarget(Action<FilePath> before, Action<FilePath> after)
    {
        if (before != null) this.beforeTargetActions.Add(before);
        if (after != null) this.afterTargetActions.Add(after);
    }

    public ProjectRunner(ICakeContext context, string projectFileGlob, Action<FilePath> projectAction, string configuration = null)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.projectFileGlob = projectFileGlob ?? throw new ArgumentNullException(nameof(projectFileGlob));
        this.projectAction = projectAction ?? throw new ArgumentNullException(nameof(projectAction));
        this.configuration = configuration
            ?? context.Argument<string>("configuration")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void Run()
    {
        foreach (var projectFile in context.GetFiles(projectFileGlob))
        {
            var testAssemblyName = projectFile.GetFilenameWithoutExtension();
            var targets = context.GetFiles(projectFile.GetDirectory() + $"/bin/{configuration}/**/{testAssemblyName}.dll");

            foreach (var target in targets)
            {
                foreach (var beforeTargetAction in beforeTargetActions)
                    beforeTargetAction.Invoke(target);
            }

            projectAction.Invoke(projectFile);

            foreach (var target in targets)
            {
                foreach (var afterTargetAction in afterTargetActions.AsEnumerable().Reverse())
                    afterTargetAction.Invoke(target);
            }
        }
    }
}
