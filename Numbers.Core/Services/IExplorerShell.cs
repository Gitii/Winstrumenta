namespace Numbers.Core.Services;

public interface IExplorerShell
{
    public IEnumerable<RecentlyUsedFile> GetRecentlyUsedFiles(params string[] extensionFilter);
    public string GetLinkTarget(string pathToLnkFile);
}
