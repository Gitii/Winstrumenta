using System.Text;
using Microsoft.CST.RecursiveExtractor;

namespace PackageInstaller.Core.Services
{
    public class DebianPackageReader : IDebianPackageReader
    {
        private static readonly IReadOnlyList<byte> DEB_FILE_HEADER = Encoding.ASCII.GetBytes(
            "!<arch>\n"
        );

        public async Task<DebianPackageMetaData> ReadMetaData(string filePath)
        {
            if (await IsDebFile(filePath) is false)
            {
                throw new ArgumentException("File is not a deb package file");
            }

            var extractor = new Extractor();

            await foreach (
                var fileEntry in extractor.ExtractAsync(
                    filePath,
                    new ExtractorOptions()
                    {
                        Recurse = true,
                        AllowFilters = new[] { "control.tar.xz", "control.tar", "control" }
                    }
                )
            )
            {
                var relPath = Path.GetRelativePath(filePath, fileEntry.FullPath);
                if (relPath == "control.tar.xz\\control.tar\\control")
                {
                    return await ReadFromControlFile(fileEntry.Content);
                }
            }

            throw new Exception("Deb package file is malformed");
        }

        public async Task<(bool isSupported, string? reason)> IsSupported(string filePath)
        {
            if (await IsDebFile(filePath))
            {
                return (true, null);
            }
            else
            {
                return (false, "Not a debian package");
            }
        }

        private async Task<DebianPackageMetaData> ReadFromControlFile(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            var controlFile = await reader.ReadToEndAsync();

            ControlFile cf = new ControlFile();
            cf.Parse(controlFile);

            return new DebianPackageMetaData()
            {
                Package = cf.GetEntryContent("Package"),
                Architecture = cf.GetEntryContent("Architecture"),
                Version = cf.GetEntryContent("Version", ""),
                Description = cf.GetEntryContent("Description"),
                AllFields = cf.Entries.ToDictionary(entry => entry.Key, entry => entry.Content)
            };
        }

        public async Task<bool> IsDebFile(string path)
        {
            await using FileStream fs = File.OpenRead(path);

            if (fs.Length < 8)
            {
                return false; // file is too small
            }

            byte[] header = new byte[8];
            await fs.ReadAsync(header, 0, 8);

            return header.SequenceEqual(DEB_FILE_HEADER);
        }
    }
}
