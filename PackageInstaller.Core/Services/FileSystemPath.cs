using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Services
{
    public record class FileSystemPath
    {
        public string UnixPath { get; init; }
        public string WindowsPath { get; init; }
    }
}
