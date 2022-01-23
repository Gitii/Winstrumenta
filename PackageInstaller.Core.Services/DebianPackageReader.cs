using System.Text;
using System.Text.RegularExpressions;
using Community.Archives.Ar;
using Community.Archives.Core;
using Community.Archives.Tar;

namespace PackageInstaller.Core.Services;

public class DebianPackageReader : IDebianPackageReader
{
    private static readonly IReadOnlyList<byte> DEB_FILE_HEADER = Encoding.ASCII.GetBytes(
        "!<arch>\n"
    );

    public async Task<DebianPackageMetaData> ReadMetaDataAsync(FileSystemPath filePath)
    {
        if (await IsDebFileAsync(filePath.WindowsPath).ConfigureAwait(false) is false)
        {
            throw new ArgumentException("File is not a deb package file");
        }

        var arExtractor = new ArArchiveReader();
        var tarExtractor = new TarArchiveReader();

        string? iconName = null;
        DebianPackageMetaData? metaData = null;

        var stream = File.OpenRead(filePath.WindowsPath);
        await foreach (
            var arEntry in arExtractor
                .GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES)
                .ConfigureAwait(false)
        )
        {
            if (arEntry.Name.StartsWith("control.", StringComparison.OrdinalIgnoreCase))
            {
                await foreach (
                    var controlEntry in tarExtractor
                        .GetFileEntriesAsync(arEntry.Content, "^./control$")
                        .ConfigureAwait(false)
                )
                {
                    metaData = await ReadFromControlFileAsync(controlEntry.Content)
                        .ConfigureAwait(false);
                }
            }
            else if (arEntry.Name.StartsWith("data.", StringComparison.OrdinalIgnoreCase))
            {
                await foreach (
                    var desktopEntry in tarExtractor
                        .GetFileEntriesAsync(arEntry.Content, "[.]desktop$")
                        .ConfigureAwait(false)
                )
                {
                    iconName = await ReadIconNameFromDesktopFileAsync(desktopEntry.Content)
                        .ConfigureAwait(false);
                }
            }
        }

        if (metaData.HasValue)
        {
            return new DebianPackageMetaData()
            {
                Package = metaData.Value.Package,
                Version = metaData.Value.Version,
                Architecture = metaData.Value.Architecture,
                Description = metaData.Value.Description,
                IconName = iconName ?? metaData.Value.IconName,
                AllFields = metaData.Value.AllFields,
            };
        }

        throw new Exception("Deb package file is malformed");
    }

    private async Task<string?> ReadIconNameFromDesktopFileAsync(Stream stream)
    {
        StreamReader reader = new StreamReader(stream);
        var desktopFile = await reader.ReadToEndAsync().ConfigureAwait(false);

        DesktopFile cf = new DesktopFile();
        cf.Parse(desktopFile);

        var desktopEntry = cf.Groups.FirstOrDefault((g) => g.Key == "Desktop Entry").AsNullable();
        if (desktopEntry.HasValue)
        {
            return desktopEntry.Value.Entries.FirstOrDefault((e) => e.Key == "Icon").Content;
        }

        return null;
    }

    public async Task<(bool isSupported, string? reason)> IsSupportedAsync(FileSystemPath filePath)
    {
        if (await IsDebFileAsync(filePath.WindowsPath).ConfigureAwait(false))
        {
            return (true, null);
        }
        else
        {
            return (false, "Not a debian package");
        }
    }

    private async Task<DebianPackageMetaData> ReadFromControlFileAsync(Stream stream)
    {
        StreamReader reader = new StreamReader(stream);
        var controlFile = await reader.ReadToEndAsync().ConfigureAwait(false);

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

    public async Task<bool> IsDebFileAsync(string path)
    {
        FileStream fs = File.OpenRead(path);
        await using var __ = fs.ConfigureAwait(false);

        if (fs.Length < 8)
        {
            return false; // file is too small
        }

        byte[] header = new byte[8];
        _ = await fs.ReadAsync(header, 0, 8).ConfigureAwait(false);

        return header.SequenceEqual(DEB_FILE_HEADER);
    }
}
