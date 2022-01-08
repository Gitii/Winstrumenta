using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PackageInstaller.Core.Services;

namespace PackageInstaller.IconThemes;

internal class IconTheme : IIconTheme
{
    private readonly Func<Task<Stream>> _iconsFileStreamFactory;
    private IDictionary<string, string> _entries = new Dictionary<string, string>();
    private string _license = String.Empty;

    public IconTheme(string name, string description, Func<Task<Stream>> iconsFileStreamFactory)
    {
        _iconsFileStreamFactory = iconsFileStreamFactory;
        Name = name;
        Description = description;
    }

    public async Task LoadFromStream(Stream mappingFileStream, Stream licenseStream)
    {
        await ReadMapping(mappingFileStream);
        await ReadLicense(licenseStream);
    }

    private async Task ReadLicense(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);

        _license = await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private async Task ReadMapping(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var (name, target) = line.Split(';');

                _entries.Add(name, target);
            }
        }
    }

    public string Name { get; }
    public string Description { get; }
    public string License => _license;

    public async Task<Stream?> GetSvgIconByName(string packageThemeName)
    {
        if (_entries.TryGetValue(packageThemeName, out string fileName))
        {
            await using var stream = await _iconsFileStreamFactory();
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = zipArchive.Entries.FirstOrDefault((e) => e.FullName == fileName);
            if (entry != null)
            {
                var buffer = new MemoryStream();
                await entry.Open().CopyToAsync(buffer);
                buffer.Position = 0;
                return buffer;
            }
        }

        return null;
    }
}
