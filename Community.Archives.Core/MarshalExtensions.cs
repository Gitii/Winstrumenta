using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Core
{
    public static class StreamMarshallingExtensions
    {
        /// <summary>
        /// Align the current stream to the specified byte boundary (based on <paramref name="assumedPosition"/> or if <c>null</c> on <seealso cref="Stream.Position"/>).
        /// </summary>
        /// <param name="stream">The stream which will be aligned</param>
        /// <param name="boundary">The boundary</param>
        /// <param name="assumedPosition"></param>
        public static async Task AlignToBoundary(
            this Stream stream,
            int boundary,
            long? assumedPosition = null
        )
        {
            var position = assumedPosition ?? stream.Position;
            var aligned = (position / boundary);
            if ((aligned * boundary) < position)
            {
                var newPosition = (aligned + 1) * boundary;
                await stream.Skip(newPosition - position);
            }
        }

        public static async Task<MemoryStream> ReadStream(this Stream stream, long length)
        {
            var outputStream = new MemoryStream(new byte[length]);

            await stream.CopyRangeToAsync(outputStream, length);

            if (outputStream.Position != length)
            {
                throw new Exception("Not all data could be read from stream");
            }

            outputStream.Position = 0;

            return outputStream;
        }

        public static async Task<T[]> ReadStruct<T>(this Stream stream, int countOfStruct)
            where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var structs = new T[countOfStruct];
            for (var i = 0; i < structs.Length; i++)
            {
                structs[i] = await ReadStruct<T>(stream).ConfigureAwait(false);
            }

            return structs;
        }

        public static Task SkipStruct<T>(this Stream stream, int countOfStruct) where T : struct
        {
            var size = Marshal.SizeOf<T>();

            return Skip(stream, countOfStruct * size);
        }

        const long SKIP_BUFFER_LENGTH = 32768;

        /// <summary>
        /// A shared buffer that is used as a sink for <seealso cref="Skip{T}"/>: data is only written but never read.
        /// Using a shared buffer avoids unnecessary allocations on each call.
        /// </summary>
        static readonly byte[] SkipBuffer = new byte[SKIP_BUFFER_LENGTH];

        public static async Task Skip(this Stream stream, long numberOfBytes)
        {
            if (stream.CanSeek)
            {
                stream.Seek(numberOfBytes, SeekOrigin.Current);
            }
            else
            {
                int read;
                long lengthLeft = numberOfBytes;
                while (
                    lengthLeft > 0
                    && (
                        read = await stream
                            .ReadAsync(SkipBuffer, 0, (int)Math.Min(SKIP_BUFFER_LENGTH, lengthLeft))
                            .ConfigureAwait(false)
                    ) > 0
                )
                {
                    lengthLeft -= read;
                }
            }
        }

        public static Task Skip<T>(Stream stream) where T : struct
        {
            return Skip(stream, Marshal.SizeOf<T>());
        }

        public static async Task<T> ReadStruct<T>(this Stream stream) where T : struct
        {
            var size = Marshal.SizeOf<T>();

            byte[] bytes = new byte[size];

            var readBytes = await stream.ReadAsync(bytes).ConfigureAwait(false);

            if (readBytes != size)
            {
                throw new Exception($"Could not read {size} bytes from stream");
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                var structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                // now fix byte order
                return structure.ConvertByteOrder();
            }
            finally
            {
                handle.Free();
            }
        }

        public static async Task<string> ReadAnsiString(this Stream stream, long length)
        {
            await using var stringStream = await stream
                .CopyRangeToAsync(length)
                .ConfigureAwait(false);
            return Encoding.ASCII.GetString(stringStream.ToArray());
        }

        public static Task<string> ReadAnsiString(this Stream stream, int expectedMaxLength = 20)
        {
            byte[] buffer = new byte[expectedMaxLength];
            int pointer = 0;
            bool endOfString = false;
            do
            {
                int dataOrEof = stream.ReadByte();
                if (dataOrEof == 0 || dataOrEof < 0)
                {
                    endOfString = true;
                }
                else
                {
                    if (buffer.Length >= pointer)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }

                    buffer[pointer++] = (byte)dataOrEof;
                }
            } while (!endOfString);

            return Task.FromResult(Encoding.ASCII.GetString(buffer, 0, pointer));
        }
    }
}