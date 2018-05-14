public Script.Builder CreateScriptBuilder() => new Script.Builder(Context);

public sealed class Script
{
    private readonly Action<Script, object> handlers;
    public ICakeContext Context { get; }
    public string Configuration { get; }

    private Script(Action<Script, object> handlers, ICakeContext context, string configuration)
    {
        this.handlers = handlers;
        Context = context;
        Configuration = configuration;
    }

    public sealed class Builder
    {
        private readonly ICakeContext context;
        private Action<Script, object> handlers;

        public Builder(ICakeContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void AddHandler<T>(Action<Script, T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            handlers += (model, message) =>
            {
                if (message is T) handler.Invoke(model, (T)message);
            };
        }

        public Script Build()
        {
            return new Script(
                handlers,
                context,
                context.Argument<string>("configuration", "Release"));
        }
    }

    public void Handle(object message)
    {
        handlers?.Invoke(this, message);
    }

    public void Test()
    {
        foreach (var projectFile in Context.GetFiles("**/*.Tests.*proj"))
        {
            var testAssemblyName = projectFile.GetFilenameWithoutExtension();
            var projectTargetFiles = Context.GetFiles(projectFile.GetDirectory() + $"/bin/{Configuration}/**/{testAssemblyName}.dll");

            foreach (var projectTargetFile in projectTargetFiles)
                Handle(new BeforeProjectTargetTestsMessage(projectTargetFile));

            Handle(new RunProjectTestsMessage(projectFile));

            foreach (var projectTargetFile in projectTargetFiles)
                Handle(new AfterProjectTargetTestsMessage(projectTargetFile));
        }
    }
}

public static MSBuildSettings DefaultMSBuildSettings(this Script script)
{
    return new MSBuildSettings()
        .SetConfiguration(script.Configuration)
        .SetVerbosity(Verbosity.Minimal);
}

#region Building

public static Script.Builder AddDotNetCoreTest(this Script.Builder builder)
{
    builder.AddHandler<RunProjectTestsMessage>((script, message) =>
    {
        script.Context.DotNetCoreTest(message.ProjectFilePath.FullPath, new DotNetCoreTestSettings
        {
            Configuration = script.Configuration,
            NoBuild = true
        });
    });

    return builder;
}

public static Script.Builder AddAltCover(this Script.Builder builder)
{
    #tool AltCover

    // https://github.com/SteveGilham/altcover/wiki/Modes-of-Operation#instrument-now-test-later-collect-coverage-after-that

    builder.AddHandler<BeforeProjectTargetTestsMessage>((script, message) =>
    {
        AltCover(
            script,
            arguments: $"--save --inplace --opencover --assemblyExcludeFilter=\"{message.ProjectTargetPath.GetFilenameWithoutExtension()}\"",
            workingDirectory: message.ProjectTargetPath.GetDirectory());
    });

    builder.AddHandler<BeforeProjectTargetTestsMessage>((script, message) =>
    {
        var directory = message.ProjectTargetPath.GetDirectory();

        AltCover(
            script,
            arguments: "runner --collect --recorderDirectory=.",
            workingDirectory: directory);

        script.Handle(new CodeCoverageFileCreatedMessage(directory.CombineWithFilePath("coverage.xml")));
    });

    return builder;

    void AltCover(Script script, string arguments, DirectoryPath workingDirectory)
    {
        script.Context.StartProcess(
            script.Context.Tools.Resolve("AltCover.exe"),
            new ProcessSettings { Arguments = arguments, WorkingDirectory = workingDirectory });
    }
}

public static Script.Builder AddCodecov(this Script.Builder builder)
{
    #tool Codecov
    #addin Cake.Codecov

    builder.AddHandler<CodeCoverageFileCreatedMessage>((script, message) =>
    {
        var settings = new CodecovSettings
        {
            Files = new[] { message.CoverageFilePath.FullPath }
        };

        // TODO: Make sure it's working with AppVeyor and SemaphoreCI

        script.Context.Codecov(settings);
    });

    return builder;
}

#endregion

#region Messages

public readonly struct BeforeProjectTargetTestsMessage
{
    public FilePath ProjectTargetPath { get; }

    public BeforeProjectTargetTestsMessage(FilePath projectTargetPath)
    {
        ProjectTargetPath = projectTargetPath;
    }
}

public readonly struct AfterProjectTargetTestsMessage
{
    public FilePath ProjectTargetPath { get; }

    public AfterProjectTargetTestsMessage(FilePath projectTargetPath)
    {
        ProjectTargetPath = projectTargetPath;
    }
}

public readonly struct RunProjectTestsMessage
{
    public FilePath ProjectFilePath { get; }

    public RunProjectTestsMessage(FilePath projectFilePath)
    {
        ProjectFilePath = projectFilePath;
    }
}

public readonly struct CodeCoverageFileCreatedMessage
{
    public FilePath CoverageFilePath { get; }

    public CodeCoverageFileCreatedMessage(FilePath coverageFilePath)
    {
        CoverageFilePath = coverageFilePath;
    }
}

#endregion
