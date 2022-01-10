namespace PackageInstaller.Core.Services
{
    public interface IPath
    {
        public string ToUnixPath(string windowsPathStyle);

        public FileSystemPath ToFileSystemPath(string windowsPath);
    }
}