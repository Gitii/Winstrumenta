namespace Community.Archives.Core;

public static class StreamExtensions
{
    public static async Task CopyRangeToAsync(this Stream input, Stream output, long length)
    {
        const long BUFFER_LENGTH = 32768;
        byte[] buffer = new byte[BUFFER_LENGTH];
        int read;
        while (
            length > 0
            && (
                read = await input
                    .ReadAsync(buffer, 0, (int)Math.Min(BUFFER_LENGTH, length))
                    .ConfigureAwait(false)
            ) > 0
        )
        {
            await output.WriteAsync(buffer, 0, read).ConfigureAwait(false);
            length -= read;
        }
    }

    public static async Task<MemoryStream> CopyRangeToAsync(this Stream input, long length)
    {
        var memStream = new MemoryStream();
        await CopyRangeToAsync(input, memStream, length);
        memStream.Position = 0;
        return memStream;
    }
}