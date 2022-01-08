namespace Community.Archives.Core;

public readonly struct ArchiveEntry : IDisposable, IAsyncDisposable
{
    public Stream Content { get; init; }

    public string Name { get; init; }

    public void Dispose()
    {
        Content?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return Content?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    public override string ToString()
    {
        return $"{Name} ({Content.Length} bytes)";
    }
}
