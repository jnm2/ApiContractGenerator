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
