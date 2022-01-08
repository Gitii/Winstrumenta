namespace Community.Archives.Core;

public interface IArchiveReader
{
    public readonly struct ArchiveMetaData
    {
        public string Package { get; init; }
        public string Version { get; init; }
        public string Architecture { get; init; }
        public string Description { get; init; }

        public IReadOnlyDictionary<string, string> AllFields { get; init; }
    }

    /// <summary>
    /// Enumerate the FileEntries in the given archive asynchronously.
    /// </summary>
    /// <param name="stream">The archive file stream</param>
    /// <returns>The ArchiveEntry found</returns>
    IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(Stream stream, params string[] regexMatcher);

    Task<ArchiveMetaData> GetMetaData(Stream stream);

    public bool SupportsMetaData { get; }

    public const string MATCH_ALL_FILES = ".*";
}
