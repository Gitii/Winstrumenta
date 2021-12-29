using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Services
{
    public interface IPath
    {
        public string ToUnixPath(string windowsPathStyle);

        public FileSystemPath ToFileSystemPath(string windowsPath);
    }
}