namespace WindowExtensions;

public class WindowPosition
{
    public int Top { get; private set; }
    public int Left { get; private set; }

    public WindowPosition(int top, int left)
    {
        this.Top = top;
        this.Left = left;
    }
}
