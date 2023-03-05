namespace Shared.Services.Implementations;

public class DisposableFiles : IDisposableFiles
{
    private ISet<string> _files = new HashSet<string>();

    public void Dispose()
    {
        DeleteFiles();
    }

    public void AddFiles(params string[] files)
    {
        foreach (var file in files)
        {
            _files.Add(file);
        }
    }

    public void DeleteFiles()
    {
        foreach (var file in _files)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Couldn't delete file {file}: {e}");
                }
            }
        }

        _files.Clear();
    }
}
