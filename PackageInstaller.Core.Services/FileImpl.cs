namespace PackageInstaller.Core.Services;

public class FileImpl : IFile
{
    public void CopyFile(string sourceFile, string destinationFile, bool overwrite = false)
    {
        File.Copy(sourceFile, destinationFile, overwrite);
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public string GetTemporaryFilePath(string? extension = null)
    {
        var extensionWithDot = string.IsNullOrEmpty(extension)
          ? ""
          : extension.StartsWith('.')
              ? extension
              : $".{extension}";

        var tempFolder = Path.GetTempPath();
        var fileName = $"{Guid.NewGuid()}{extensionWithDot}";

        return Path.Join(tempFolder, fileName);
    }
}
