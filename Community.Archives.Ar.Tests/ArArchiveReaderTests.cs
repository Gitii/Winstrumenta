using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Community.Archives.Core;
using NUnit.Framework;

namespace Community.Archives.Ar.Tests
{
    public class ArArchiveReaderTests
    {
        [Test]
        public async Task Test_GetMetaData()
        {
            var reader = new ArArchiveReader();
            var data = await reader.GetMetaData(
                File.OpenRead(
                    @"C:\Users\Germi\Downloads\firefox_95.0.1+build2-0ubuntu0.20.04.1_amd64.deb"
                )
            );
        }

        [Test]
        public async Task Test_GetEntries()
        {
            var reader = new ArArchiveReader();
            var stream = File.OpenRead(
                @"C:\Users\Germi\Downloads\firefox_95.0.1+build2-0ubuntu0.20.04.1_amd64.deb"
            );

            await foreach (var entry in reader.GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES))
            {
                Debugger.Break();
            }
        }
    }
}