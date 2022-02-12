namespace PackageInstaller.Core.Services;

public class MouseEventExtArgs : EventArgs
{
    public MouseEventExtArgs(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }
}
