namespace Shared.Services;

public interface IFile
{
    public void CopyFile(string sourceFile, string destinationFile, bool overwrite = false);

    /// <inheritdoc cref="File.Exists"/>
    public bool Exists(string path);

    public string GetTemporaryFilePath(string? extension = null);

    /// <inheritdoc cref="File.Delete"/>
    public void Delete(string path);

    /// <inheritdoc cref="File.OpenWrite"/>
    public FileStream OpenRead(string path);
}
