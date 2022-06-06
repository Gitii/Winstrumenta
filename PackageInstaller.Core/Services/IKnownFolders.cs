namespace PackageInstaller.Core.Services;

public interface IKnownFolders
{
    public string GetPath(KnownFolder knownFolder);
}
