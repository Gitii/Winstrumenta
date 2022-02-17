using System.Collections.ObjectModel;

namespace PackageInstaller.Core.Services;

public class ControlFile
{
    public readonly struct Entry
    {
        public readonly string Key;
        public readonly string Content;

        public Entry(string key, string content)
        {
            Key = key;
            Content = content;
        }

        public Entry? AsNullable()
        {
            if (Key == null && Content == null)
            {
                return null;
            }

            return this;
        }

        public override string? ToString()
        {
            if (Key == null && Content == null)
            {
                return "<Empty>";
            }

            return $"{Key} = {Content}";
        }
    }

    private IList<Entry> _entries = new List<Entry>();

    public IReadOnlyList<Entry> Entries => new ReadOnlyCollection<Entry>(_entries);

    public void Parse(string contentFile)
    {
        _entries.Clear();
        var lines = contentFile.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        string key = string.Empty;
        string content = string.Empty;

        foreach (string line in lines)
        {
            if (line.StartsWith(" ", StringComparison.InvariantCulture))
            {
                if (key.Length == 0)
                {
                    throw new Exception("Malformed control file: content before key");
                }

                content += "\n" + line.TrimStart();
            }
            else if (line.Contains(":", StringComparison.InvariantCulture))
            {
                if (key.Length > 0)
                {
                    _entries.Add(new Entry(key, content.Trim()));
                    key = string.Empty;
                    content = string.Empty;
                }

                var index = line.IndexOf(":", StringComparison.Ordinal);
                key = line.Substring(0, index);
                content = line.Substring(index + 1).TrimStart();
            }
            else if (line.StartsWith("#", StringComparison.Ordinal))
            {
                // ignore, comment
            }
            else
            {
                throw new Exception("Malformed control file: neither content nor key");
            }
        }

        if (key.Length > 0)
        {
            _entries.Add(new Entry(key, content.Trim()));
        }
    }

    public string GetEntryContent(string key, string? defaultValue = null)
    {
        Entry? entry = Entries.FirstOrDefault((e) => e.Key == key).AsNullable();
        if (!entry.HasValue && defaultValue == null)
        {
            throw new KeyNotFoundException(key);
        }
        else if (!entry.HasValue)
        {
            return defaultValue!;
        }

        return (entry.Value.Content ?? defaultValue)!;
    }
}
