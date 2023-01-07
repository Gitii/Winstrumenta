using System.Text;

namespace Numbers.Core.Services;

public record struct FileEncoding
{
    public string Delimiter { get; set; }
    public char QuoteCharacter { get; set; }
    public Encoding Encoding { get; set; }
}
