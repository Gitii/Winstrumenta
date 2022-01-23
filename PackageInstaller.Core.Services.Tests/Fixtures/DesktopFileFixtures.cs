using System;
using System.Collections.Generic;

namespace PackageInstaller.Core.Services.Tests.Fixtures;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "MA0069:Non-constant static fields should not be visible",
    Justification = "<Pending>"
)]
public class DesktopFileFixtures
{
    public static string SingleGroupNoEntriesRaw = "[groupname]";

    public static IList<DesktopFile.Group> SingleGroupNoEntries = new List<DesktopFile.Group>()
    {
        new DesktopFile.Group("groupname", Array.Empty<DesktopFile.Entry>())
    };

    public static string SingleGroupSingleEntryRaw = "[groupname]\nkey=value";

    public static IList<DesktopFile.Group> SingleGroupSingleEntry = new List<DesktopFile.Group>()
    {
        new DesktopFile.Group("groupname", new[] { new DesktopFile.Entry("key", "value") })
    };

    public static string MultipleGroupsSingleEntryRaw =
        "[groupname]\nkey=value\n[groupname2]\nkey=value";

    public static IList<DesktopFile.Group> MultipleGroupsSingleEntry = new List<DesktopFile.Group>()
    {
        new DesktopFile.Group("groupname", new[] { new DesktopFile.Entry("key", "value") }),
        new DesktopFile.Group("groupname2", new[] { new DesktopFile.Entry("key", "value") }),
    };
}
