namespace Numbers.Core.Services;

public class RecentlyUsedFile
{
    public string FileName { get; set; } = String.Empty;
    public string FullPath { get; set; } = String.Empty;
    public DateTime AccessTime { get; set; } = DateTime.Now;
}
