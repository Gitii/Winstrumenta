using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Services
{
    public interface IThemeManager
    {
        void SetTitleBarColor(byte a, byte r, byte g, byte b);
        void ResetTitleBarColor();
    }
}
