namespace Numbers.Core.Services;

public interface IGlobalContext
{
    public ICsvTable? Table { get; set; }
    public string? FilePath { get; set; }
    public FileEncoding? FileEncoding { get; set; }
}
