namespace Shared.Services.Implementations;

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

    public void Delete(string path)
    {
        File.Delete(path);
    }

    public FileStream OpenRead(string path)
    {
        return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
