using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PackageInstaller.Core.Helpers;

namespace PackageInstaller.Core.Services.WinUI;

public class IconTheme : IIconTheme
{
    private readonly Func<Task<Stream>> _iconsFileStreamFactory;
    private string? _mapping;
    private string _license = String.Empty;

    public IconTheme(string name, string description, Func<Task<Stream>> iconsFileStreamFactory)
    {
        _iconsFileStreamFactory = iconsFileStreamFactory;
        Name = name;
        Description = description;
    }

    public async Task LoadFromStreamAsync(Stream mappingFileStream, Stream licenseStream)
    {
        await ReadMappingAsync(mappingFileStream).ConfigureAwait(false);
        await ReadLicenseAsync(licenseStream).ConfigureAwait(false);
    }

    private async Task ReadLicenseAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);

        _license = await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private async Task ReadMappingAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);

        _mapping = await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    public string Name { get; }
    public string Description { get; }
    public string License => _license;

    private bool TryFindMapping(string iconName, out string fileName)
    {
        var iconNameSpan = iconName.AsSpan();
        foreach (var (line, _) in _mapping.AsSpan().SplitLines())
        {
            var (key, value) = line.Split(';');

            if (key.Equals(iconNameSpan, StringComparison.Ordinal))
            {
                fileName = value.ToString();
                return true;
            }
        }

        fileName = String.Empty;
        return true;
    }

    public async Task<Stream?> GetSvgIconByNameAsync(string packageThemeName)
    {
        if (TryFindMapping(packageThemeName, out string fileName))
        {
            var stream = await _iconsFileStreamFactory().ConfigureAwait(false);
            await using var _ = stream.ConfigureAwait(false);
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = zipArchive.Entries.FirstOrDefault((e) => e.FullName == fileName);
            if (entry != null)
            {
                var buffer = new MemoryStream();
                await entry.Open().CopyToAsync(buffer).ConfigureAwait(false);
                buffer.Position = 0;
                return buffer;
            }
        }

        return null;
    }
}
