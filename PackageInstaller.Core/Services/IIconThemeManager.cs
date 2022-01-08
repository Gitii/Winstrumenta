using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageInstaller.Core.Services
{
    public interface IIconThemeManager
    {
        public IReadOnlyList<IIconTheme> AvailableThemes { get; }

        public IIconTheme ActiveIconTheme { get; set; }

        public Task LoadThemes();
    }
}
