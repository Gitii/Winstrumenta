using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Community.Archives.Core;
using NUnit.Framework;

namespace Community.Archives.Tar.Tests
{
    public class TarArchiveReaderTests
    {
        [Test]
        public async Task Test_GetEntries()
        {
            var reader = new TarArchiveReader();
            var stream = File.OpenRead(
                @"C:\Users\Germi\Downloads\control.tar.gz"
            );

            await foreach (
                var entry in reader.GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES)
            )
            {
                Debugger.Break();
            }
        }
    }
}