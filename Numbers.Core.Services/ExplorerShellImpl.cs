using ShellLink;

namespace Numbers.Core.Services;

public class ExplorerShellImpl : IExplorerShell
{
    public IEnumerable<RecentlyUsedFile> GetRecentlyUsedFiles(params string[] extensionFilter)
    {
        var rufs = Directory.EnumerateFiles(
            Environment.GetFolderPath(Environment.SpecialFolder.Recent)
        );

        return rufs.Where(
                (f) =>
                {
                    const string LINK_FILE_EXTENSION = ".lnk";
                    var cleanFileExtension = Path.GetExtension(
                        f.Substring(0, f.Length - LINK_FILE_EXTENSION.Length)
                    );

                    return extensionFilter.Length == 0
                        || extensionFilter.Contains(
                            cleanFileExtension,
                            StringComparer.OrdinalIgnoreCase
                        );
                }
            )
            .Select(
                (f) =>
                {
                    var realFile = GetLinkTarget(f);

                    return new RecentlyUsedFile()
                    {
                        FileName = Path.GetFileName(realFile),
                        FullPath = realFile,
                        AccessTime = File.GetLastAccessTime(realFile),
                    };
                }
            );
    }

    public string GetLinkTarget(string pathToLnkFile)
    {
        return Shortcut.ReadFromFile(pathToLnkFile).LinkTargetIDList.Path;
    }
}
