using System.Collections.Generic;

namespace PackageInstaller.Core.Services.Tests.ControlFileFixtures
{
    public class ControlFileFixtures
    {
        public static string SingleEntryRaw = "Source: dpkg";
        public static IList<ControlFile.Entry> SingleEntry = new List<ControlFile.Entry>()
        {
            new ControlFile.Entry("Source", "dpkg")
        };

        public static string SingleMultilineEntryRaw =
            "Build-Depends:\n dpkg-dev (>= 1.17.14),\n debhelper (>= 9.20141010),\n pkg-config,\n gettext (>= 0.19),\n po4a (>= 0.43),\n zlib1g-dev,\n libbz2-dev,\n liblzma-dev,\n libselinux1-dev [linux-any],\n libncursesw5-dev,\n libio-string-perl <!nocheck>,";

        public static IList<ControlFile.Entry> SingleMultilineEntry = new List<ControlFile.Entry>()
        {
            new ControlFile.Entry(
                "Build-Depends",
                "dpkg-dev (>= 1.17.14),\ndebhelper (>= 9.20141010),\npkg-config,\ngettext (>= 0.19),\npo4a (>= 0.43),\nzlib1g-dev,\nlibbz2-dev,\nliblzma-dev,\nlibselinux1-dev [linux-any],\nlibncursesw5-dev,\nlibio-string-perl <!nocheck>,"
            )
        };

        public static string SingleMultilineEntry2Raw =
            "Description: Debian package management static library\n This package provides the header files and static library necessary to\n develop software using libdpkg, the same library used internally by dpkg.\n .\n Note though, that the API is to be considered volatile, and might change\n at any time, use at your own risk.";

        public static IList<ControlFile.Entry> SingleMultiline2Entry = new List<ControlFile.Entry>()
        {
            new ControlFile.Entry(
                "Description",
                "Debian package management static library\nThis package provides the header files and static library necessary to\ndevelop software using libdpkg, the same library used internally by dpkg.\n.\nNote though, that the API is to be considered volatile, and might change\nat any time, use at your own risk."
            )
        };
    }
}
