namespace Shared.Services;

public interface IDisposableFiles : IDisposable
{
    public void AddFiles(params string[] files);
    public void DeleteFiles();
}
