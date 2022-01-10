using System.Collections.ObjectModel;

namespace PackageInstaller.Core.Services;

public class DesktopFile
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

        public override bool Equals(object? obj)
        {
            if (obj is not Entry)
            {
                return false;
            }

            Entry entry = (Entry)obj;

            return Key == entry.Key && Content == entry.Content;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Content);
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

    public readonly struct Group
    {
        public readonly string Key;
        public readonly Entry[] Entries;

        public Group(string key, Entry[] entries)
        {
            Key = key;
            Entries = entries;
        }

        public Group? AsNullable()
        {
            if (Key == null && Entries == null)
            {
                return null;
            }

            return this;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Group)
            {
                return false;
            }

            Group group = (Group)obj;

            return Key == group.Key && Entries.SequenceEqual(group.Entries);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Entries);
        }

        public override string? ToString()
        {
            if (Key == null && Entries == null)
            {
                return "<Empty>";
            }

            return $"{Key} ({Entries.Length} entries)";
        }
    }

    private IList<Group> _groups = new List<Group>();

    public IReadOnlyList<Group> Groups => new ReadOnlyCollection<Group>(_groups);

    public void Parse(string content)
    {
        _groups.Clear();
        var lines = content.Split(
            "\n",
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        List<Entry> currentEntries = new List<Entry>();
        string currentGroupKey = String.Empty;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            {
                // skip comments and empty lines
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                AddGroupIfKeyNotEmpty();

                currentGroupKey = line.Substring(1, line.Length - 2);
            }
            else if (line.Contains('='))
            {
                var (key, value) = line.Split('=');

                currentEntries.Add(new Entry(key, value));
            }
            else
            {
                throw new Exception($"Invalid line {line}");
            }
        }

        AddGroupIfKeyNotEmpty();

        void AddGroupIfKeyNotEmpty()
        {
            if (currentGroupKey != String.Empty)
            {
                _groups.Add(new Group(currentGroupKey, currentEntries.ToArray()));
                currentEntries.Clear();
            }

            currentGroupKey = String.Empty;
            currentEntries.Clear();
        }
    }
}