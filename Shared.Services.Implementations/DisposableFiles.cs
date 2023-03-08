namespace Shared.Services.Implementations;

public class DisposableFiles : IDisposableFiles
{
    private readonly IFile _file;
    private readonly ISet<string> _files;

    public DisposableFiles(IFile file)
    {
        _file = file;
        _files = new HashSet<string>();
    }

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
            if (_file.Exists(file))
            {
                try
                {
                    _file.Delete(file);
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
