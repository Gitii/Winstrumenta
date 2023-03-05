namespace Shared.Services;

public interface IPath
{
    /// <summary>
    /// Converts a windows path (with a driver letter) to a unix-like path which can be used by wsl to access content in windows.
    /// </summary>
    /// <param name="windowsPath">A valid windows paths which starts with with a driver letter.</param>
    /// <returns></returns>
    public string ToUnixPath(string windowsPath);

    /// <summary>
    /// Converts an absolute unix- or windows-style path to <see cref="FileSystemPath"/> instance.
    /// </summary>
    /// <param name="path">A valid and absolute unix or windows path which starts with with a driver letter.</param>
    /// <returns>An instance of <see cref="FileSystemPath"/>.</returns>
    public FileSystemPath ToFileSystemPath(string path);

    /// <summary>
    /// Converts an absolute unix path (with either a windows mount /mnt/) or an internal fs one) to a windows path.
    /// Paths to the root fs ("/" but not "/mnt") will be converted to the wsl-network share \\wsl$.
    /// </summary>
    /// <param name="unixPath">An absolute unix path.</param>
    /// <returns>An instance of <see cref="FileSystemPath"/>.</returns>
    string ToWindowsPath(string unixPath);

    bool IsWslNetworkShare(string path);

    /// <summary>
    /// Checks if the passed in path is a network path (either a unc path or a path to a network drive).
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <returns></returns>
    bool IsNetworkShare(string path);

    string GetFullPath(string path);
}
