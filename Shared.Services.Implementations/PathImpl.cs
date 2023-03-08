namespace Shared.Services.Implementations;

public class PathImpl : IPath
{
    public string ToUnixPath(string windowsPath)
    {
        if (string.IsNullOrEmpty(windowsPath))
        {
            throw new Exception("Path must not be empty or null");
        }

        if (IsWslNetworkShare(windowsPath))
        {
            throw new Exception("Cannot convert a wsl share to a unix path");
        }

        if (windowsPath.Length < 2 || !char.IsLetter(windowsPath[0]) || windowsPath[1] != ':')
        {
            throw new Exception(
                $"The path is not rooted to a drive with a drive letter: {windowsPath}"
            );
        }

        var pathWithoutRoot = windowsPath
            .Substring(2)
            .TrimStart(
                Path.PathSeparator,
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );

        return $"{ToUnixMount(windowsPath)}/{pathWithoutRoot.Replace('\\', '/')}";
    }

    private string ToUnixMount(string root)
    {
        var driveLetter = root[0];
        return $"/mnt/{char.ToLowerInvariant(driveLetter)}";
    }

    private bool IsUnixMountedPath(string path)
    {
        return path.StartsWith("/");
    }

    public FileSystemPath ToFileSystemPath(string path)
    {
        if (IsUnixMountedPath(path))
        {
            return ConvertFromUnixPath(path);
        }
        else if (IsWindowsPath(path))
        {
            return ConvertFromWindowsPath(path);
        }
        else
        {
            throw new Exception($"Invalid path '{path}'");
        }

        FileSystemPath ConvertFromUnixPath(string unixPath)
        {
            return new FileSystemPath(unixPath, ToWindowsPath(unixPath));
        }

        FileSystemPath ConvertFromWindowsPath(string windowsPath)
        {
            return new FileSystemPath(ToUnixPath(windowsPath), windowsPath);
        }
    }

    public bool IsWslNetworkShare(string path)
    {
        return path.StartsWith("\\\\wsl$\\") || path.StartsWith("\\\\wsl.localhost\\");
    }

    public bool IsNetworkShare(string path)
    {
        if (!path.StartsWith(@"/") && !path.StartsWith(@"\"))
        {
            string? rootPath = System.IO.Path.GetPathRoot(path); // get drive's letter
            if (string.IsNullOrEmpty(rootPath))
            {
                return false;
            }

            System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(rootPath); // get info about the drive
            return driveInfo.DriveType == DriveType.Network; // return true if a network drive
        }

        return true; // is a UNC path
    }

    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    public string ToWindowsPath(string unixPath)
    {
        if (string.IsNullOrEmpty(unixPath))
        {
            throw new Exception("Path must not be empty or null");
        }

        if (unixPath.StartsWith("/mnt/"))
        {
            var driveLetter = unixPath.Substring(5, 1);
            var pathWithoutMount = unixPath.Substring(6);

            return $"{driveLetter}:{pathWithoutMount}".Replace('/', '\\');
        }

        throw new Exception(
            "A path to the root fs of a unix distro cannot be converted to a windows path"
        );
    }

    private bool IsWindowsPath(string path)
    {
        return path.Length > 2 && char.IsLetter(path[0]) && path[1] == ':';
    }
}
