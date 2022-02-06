namespace PackageInstaller.Core.Services;

public interface IThemeManager
{
    void SetTitleBarColor(byte a, byte r, byte g, byte b);
    void ResetTitleBarColor();
}
