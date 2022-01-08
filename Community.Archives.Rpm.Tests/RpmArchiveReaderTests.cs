using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests
{
    public class RpmArchiveReaderTests
    {
        [Test]
        public async Task Test_GetMetaData()
        {
            var reader = new RpmArchiveReader();
            var data = await reader.GetMetaData(
                File.OpenRead(@"C:\Users\Germi\Downloads\gh_2.4.0_linux_amd64.rpm")
            );
        }
    }
}