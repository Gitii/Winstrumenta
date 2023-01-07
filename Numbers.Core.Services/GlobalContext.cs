namespace Numbers.Core.Services;

public class GlobalContext : IGlobalContext
{
    public ICsvTable? Table { get; set; }
    public string? FilePath { get; set; }
    public FileEncoding? FileEncoding { get; set; }
}
