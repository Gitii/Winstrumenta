using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;

namespace Numbers.Core.ModelViews;

public class SaveViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IGlobalContext _context;
    private readonly ICsv _csv;
    public string Id { get; } = nameof(SaveViewModel);

    string _delimiter = String.Empty;

    public string Delimiter
    {
        get { return _delimiter; }
        set { this.RaiseAndSetIfChanged(ref _delimiter, value); }
    }

    ImmutableList<string> _delimiterOptions = ImmutableList<string>.Empty;

    public ImmutableList<string> DelimiterOptions
    {
        get { return _delimiterOptions; }
        set { this.RaiseAndSetIfChanged(ref _delimiterOptions, value); }
    }

    char _quoteCharacter = '\0';

    public char QuoteCharacter
    {
        get { return _quoteCharacter; }
        set { this.RaiseAndSetIfChanged(ref _quoteCharacter, value); }
    }

    Encoding _encoding = Encoding.Default;

    public Encoding Encoding
    {
        get { return _encoding; }
        set { this.RaiseAndSetIfChanged(ref _encoding, value); }
    }

    ImmutableList<Encoding> _encodingOptions = ImmutableList<Encoding>.Empty;

    public ImmutableList<Encoding> EncodingOptions
    {
        get { return _encodingOptions; }
        set { this.RaiseAndSetIfChanged(ref _encodingOptions, value); }
    }

    string _fileName = String.Empty;

    public string FileName
    {
        get { return _fileName; }
        set { this.RaiseAndSetIfChanged(ref _fileName, value); }
    }

    string _filePath = String.Empty;

    public string FilePath
    {
        get { return _filePath; }
        set { this.RaiseAndSetIfChanged(ref _filePath, value); }
    }

    public readonly struct NavigationParameter
    {
        public string FilePath { get; init; }
    }

    public SaveViewModel(IGlobalContext context, ICsv csv)
    {
        _context = context;
        _csv = csv;

        EncodingOptions = ImmutableList<Encoding>.Empty.AddRange(
            new[]
            {
                System.Text.Encoding.ASCII,
                Encoding.BigEndianUnicode,
                Encoding.Latin1,
                Encoding.UTF32,
                Encoding.UTF8,
                Encoding.Unicode
            }
        );
        DelimiterOptions = ImmutableList<string>.Empty.AddRange(new[] { ",", ";", "\t", " " });

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAsAsync);
    }

    private Task SaveAsync()
    {
        UpdateGlobalContext();

        return _csv.SaveAsAsync(_context);
    }

    private void UpdateGlobalContext()
    {
        _context.FileEncoding = new FileEncoding()
        {
            Encoding = this.Encoding,
            QuoteCharacter = QuoteCharacter,
            Delimiter = Delimiter
        };
        _context.FilePath = Path.Combine(FilePath, FileName);
    }

    private Task SaveAsAsync()
    {
        UpdateGlobalContext();

        return _csv.SaveAsAsync(_context);
    }

    public IObservable<Unit> WhenNavigatedTo(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatedFrom(INavigationParameter parameter)
    {
        UpdateGlobalContext();

        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatingTo(INavigationParameter parameter)
    {
        var contextFileEncoding =
            _context.FileEncoding
            ?? throw new Exception("Expected global context to have file encoding");

        var filePath =
            _context.FilePath ?? throw new Exception("Expected global context to have file path");

        Delimiter = contextFileEncoding.Delimiter;
        QuoteCharacter = contextFileEncoding.QuoteCharacter;
        Encoding = contextFileEncoding.Encoding;

        FileName = Path.GetFileName(filePath);
        FilePath = Path.GetDirectoryName(filePath) ?? "";

        return Observable.Return(Unit.Default);
    }

    public ICommand SaveCommand { get; }

    public ICommand SaveAsCommand { get; }
}
