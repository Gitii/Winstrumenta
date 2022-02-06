using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PackageInstaller.Core.Services;

namespace PackageInstaller.Themes;

public class ThemeManager : IThemeManager
{
    private Panel? _titleBar;

    public void SetPanel(Panel titleBar)
    {
        _titleBar = titleBar;
    }

    public void SetTitleBarColor(byte a, byte r, byte g, byte b)
    {
        if (_titleBar != null)
        {
            _titleBar.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }
    }

    public void ResetTitleBarColor()
    {
        if (_titleBar != null)
        {
            _titleBar.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
