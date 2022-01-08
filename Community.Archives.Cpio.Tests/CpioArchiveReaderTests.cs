using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Community.Archives.Cpio.Tests
{
    public class RpmArchiveReaderTests
    {
        [Test]
        public async Task Test_GetMetaData()
        {
            var reader = new CpioArchiveReader();
            await foreach (
                var entry in reader.GetFileEntriesAsync(
                    File.OpenRead(@"C:\Users\Germi\Downloads\gh_2.4.0_linux_amd64.cpio")
                )
            )
            {
                var name = entry.Name;
            }
        }
    }
}
