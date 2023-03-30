using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Windows.Input;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

namespace Numbers.Core.ModelViews;

public class TableViewModel : ReactiveObject, IViewModel, INavigable, IWindowInterface
{
    private readonly IFile _file;
    private readonly ICsv _csv;
    private readonly IGlobalContext _context;
    private readonly IExcel _excel;

    private readonly IParameterViewStackService _viewStackService;
    public string Id { get; } = nameof(TableViewModel);

    ImmutableList<(string header, string fieldName)> _columns;

    public ImmutableList<(string header, string fieldName)> Columns
    {
        get { return _columns; }
        private set { this.RaiseAndSetIfChanged(ref _columns, value); }
    }

    ImmutableList<ICsvRow> _rows;

    public ImmutableList<ICsvRow> Rows
    {
        get { return _rows; }
        private set { this.RaiseAndSetIfChanged(ref _rows, value); }
    }

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

    public TableViewModel(
        IParameterViewStackService viewStackService,
        IFile file,
        ICsv csv,
        IGlobalContext context,
        IExcel excel
    )
    {
        _viewStackService = viewStackService;
        _file = file;
        _csv = csv;
        _context = context;
        _excel = excel;

        _columns = ImmutableList<(string header, string fieldName)>.Empty;
        _rows = ImmutableList<ICsvRow>.Empty;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        OpenInExcelCommand = ReactiveCommand.CreateFromTask(OpenInExcelAsync);

        Commands = ImmutableList.Create(
            new CommandDescription()
            {
                Icon = "\uE105",
                Label = "Save",
                KeyboardAcceleratorKey = "S",
                KeyboardAcceleratorModifier = "Control",
                Command = SaveCommand
            },
            new CommandDescription()
            {
                Label = "Open in Excel",
                Icon = "\uE8A7",
                KeyboardAcceleratorKey = "E",
                KeyboardAcceleratorModifier = "Control",
                Command = OpenInExcelCommand
            }
        );

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
    }

    private Task OpenInExcelAsync()
    {
        return _excel.OpenAsync(
            _context.FilePath ?? throw new Exception("Global context is missing file path")
        );
    }

    private Task SaveAsync()
    {
        if (this.Delimiter.Length == 0 || this.QuoteCharacter == '\0')
        {
            return Task.CompletedTask;
        }

        try
        {
            UpdateContext(
                _context.Table!,
                new FileEncoding()
                {
                    Delimiter = this.Delimiter,
                    Encoding = Encoding,
                    QuoteCharacter = QuoteCharacter,
                }
            );

            return _csv.SaveAsAsync(_context);
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();

            return Task.CompletedTask;
        }
    }

    public ICommand SaveCommand { get; }

    public ICommand OpenInExcelCommand { get; }

    public IObservable<Unit> WhenNavigatedTo(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatedFrom(INavigationParameter parameter)
    {
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> WhenNavigatingTo(INavigationParameter parameter)
    {
        if (_context.Table != null)
        {
            return RestoreFromContext(_context.Table);
        }

        return ReadFromFile(parameter);
    }

    private IObservable<Unit> ReadFromFile(INavigationParameter parameter)
    {
        return ProcessAsync(
                _context.FilePath
                    ?? throw new ArgumentException("Expected global context to have file path")
            )
            .ToObservable(RxApp.MainThreadScheduler);
    }

    private IObservable<Unit> RestoreFromContext(ICsvTable data)
    {
        Columns = data.HeaderNames.Zip(data.FieldNames).ToImmutableList();
        Rows = data.Rows.ToImmutableList();

        var contextFileEncoding =
            _context.FileEncoding
            ?? throw new Exception("Expected global context to have file encoding");

        Delimiter =
            DelimiterOptions.FirstOrDefault(
                (del) =>
                    del.Equals(contextFileEncoding.Delimiter, StringComparison.OrdinalIgnoreCase)
            ) ?? contextFileEncoding.Delimiter;
        QuoteCharacter = contextFileEncoding.QuoteCharacter;
        Encoding = contextFileEncoding.Encoding;

        WindowTitle = Path.GetFileName(_context.FilePath ?? "unknown.csv");

        return Observable.Return(Unit.Default);
    }

    public async Task ProcessAsync(string filePath)
    {
        try
        {
            await Task.Delay(10).ConfigureAwait(true);

            await using var stream = _file.OpenRead(filePath);

            var (data, encoding) = await _csv.ParseAsync(stream).ConfigureAwait(true);

            UpdateContext(data, encoding);
            RestoreFromContext(data);
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
    }

    private void UpdateContext(ICsvTable data, FileEncoding encoding)
    {
        _context.Table = data;
        _context.FileEncoding = encoding;
    }

    string? _windowTitle;

    public string? WindowTitle
    {
        get { return _windowTitle; }
        set { this.RaiseAndSetIfChanged(ref _windowTitle, value); }
    }

    IImmutableList<CommandDescription>? _commands;

    public IImmutableList<CommandDescription>? Commands
    {
        get { return _commands; }
        set { this.RaiseAndSetIfChanged(ref _commands, value); }
    }
}
