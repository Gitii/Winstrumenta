using Community.Archives.Core;
using SharpCompress.Archives.GZip;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.LZMA;
using SharpCompress.Compressors.Xz;
using CompressionMode = SharpCompress.Compressors.CompressionMode;
using GZipStream = SharpCompress.Compressors.Deflate.GZipStream;

namespace Community.Archives.Tar;

public class TarArchiveReader : IArchiveReader
{
    private const int BLOCK_SIZE = 512;

    public async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
        Stream stream,
        params string[] regexMatcher
    )
    {
        stream = await DecompressStream(stream);

        bool endOfFile = false;
        do
        {
            var header = await stream.ReadStruct<Header>().ConfigureAwait(false);
            if (header.IsEmpty())
            {
                endOfFile = true;
            }
            else
            {
                string fileName = header.GetName();
                var size = header.FileSize.DecodeStringAsOctalLong();
                if (header.IsFile() && regexMatcher.IsMatch(fileName))
                {
                    yield return new ArchiveEntry()
                    {
                        Content = await stream.CopyRangeToAsync(size).ConfigureAwait(false),
                        Name = fileName
                    };
                }
                else
                {
                    await stream.Skip(size);
                }

                // Stream.Position isn't reliable because we could be decompressing the stream
                // Use "assumed position" instead
                await stream.AlignToBoundary(BLOCK_SIZE, size);
            }
        } while (!endOfFile);
    }

    private async Task<Stream> DecompressStream(Stream stream)
    {
        BufferedStream rewindableStream = new BufferedStream(stream, BLOCK_SIZE);
        var oldPosition = rewindableStream.Position;

        if (GZipArchive.IsGZipFile(rewindableStream))
        {
            rewindableStream.Position = oldPosition;
            return new GZipStream(rewindableStream, CompressionMode.Decompress);
        }

        rewindableStream.Position = oldPosition;
        if (BZip2Stream.IsBZip2(rewindableStream))
        {
            rewindableStream.Position = oldPosition;
            return new BZip2Stream(rewindableStream, CompressionMode.Decompress, false);
        }

        rewindableStream.Position = oldPosition;
        if (LZipStream.IsLZipFile(rewindableStream))
        {
            rewindableStream.Position = oldPosition;
            return new LZipStream(rewindableStream, CompressionMode.Decompress);
        }

        rewindableStream.Position = oldPosition;
        if (XZStream.IsXZStream(rewindableStream))
        {
            rewindableStream.Position = oldPosition;
            return new XZStream(rewindableStream);
        }

        // assume uncompressed archive
        rewindableStream.Position = oldPosition;
        return rewindableStream;
    }

    public Task<IArchiveReader.ArchiveMetaData> GetMetaData(Stream stream)
    {
        throw new NotImplementedException();
    }

    public bool SupportsMetaData { get; } = false;
}