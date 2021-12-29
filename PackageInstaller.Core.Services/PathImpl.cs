namespace PackageInstaller.Core.Services;

public class PathImpl : IPath
{
    public string ToUnixPath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Path.IsPathFullyQualified(path))
        {
            throw new Exception("Only fully qualified paths are supported: " + (path ?? "null"));
        }

        var root = Path.GetPathRoot(path);

        if (root == null || root.Length < 2 || root.Length >= 2 && root[1] != ':')
        {
            throw new Exception("The path is not rooted to a drive with a drive letter: " + root);
        }

        var pathWithoutRoot = path.Substring(root.Length)
            .TrimStart(Path.PathSeparator, Path.AltDirectorySeparatorChar);

        return $"{ToUnixMount(root)}/{ToUnixPathSeparators(pathWithoutRoot)}";
    }

    private string ToUnixPathSeparators(string pathWithoutRoot)
    {
        return pathWithoutRoot.Replace('\\', '/');
    }

    private string ToUnixMount(string root)
    {
        var driveLetter = root[0];
        return $"/mnt/{char.ToLowerInvariant(driveLetter)}";
    }

    public FileSystemPath ToFileSystemPath(string windowsPath)
    {
        windowsPath = Path.GetFullPath(windowsPath);

        if (!File.Exists(windowsPath))
        {
            throw new FileNotFoundException(
                "Cannot convert to FileSystemPath: File doesn't exist or path is invalid",
                windowsPath
            );
        }

        return new FileSystemPath()
        {
            UnixPath = ToUnixPath(windowsPath),
            WindowsPath = windowsPath
        };
    }
}