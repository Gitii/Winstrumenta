namespace Numbers.Core.Services.Implementations;

public class GlobalContext : IGlobalContext
{
    public ICsvTable? Table { get; set; }
    public string? FilePath { get; set; }
    public FileEncoding? FileEncoding { get; set; }
}
