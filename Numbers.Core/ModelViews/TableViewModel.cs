using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Services;

namespace Numbers.Core.ModelViews;

public class TableViewModel : ReactiveObject, IViewModel, INavigable, IWindowInterface
{
    private readonly IFile _file;
    private readonly ICsv _csv;
    private readonly IGlobalContext _context;

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

    public TableViewModel(
        IParameterViewStackService viewStackService,
        IFile file,
        ICsv csv,
        IGlobalContext context
    )
    {
        _viewStackService = viewStackService;
        _file = file;
        _csv = csv;
        _context = context;

        _columns = ImmutableList<(string header, string fieldName)>.Empty;
        _rows = ImmutableList<ICsvRow>.Empty;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);

        Commands = ImmutableList.Create(
            new CommandDescription()
            {
                Icon = "\uE105",
                Label = "Save",
                KeyboardAcceleratorKey = "S",
                KeyboardAcceleratorModifier = "Control",
                Command = SaveCommand
            },
            new CommandDescription() { Label = "Open in Excel", Command = SaveCommand }
        );
    }

    private Task SaveAsync()
    {
        return _csv.SaveAsAsync(_context);
    }

    public ICommand SaveCommand { get; }

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

        WindowTitle = Path.GetFileName(_context.FilePath ?? "unknown.csv");

        return Observable.Return(Unit.Default);
    }

    public async Task ProcessAsync(string filePath)
    {
        await Task.Delay(10).ConfigureAwait(true);

        await using var stream = _file.OpenRead(filePath);

        var (data, encoding) = await _csv.ParseAsync(stream).ConfigureAwait(true);

        UpdateContext(data, encoding);
        RestoreFromContext(data);
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
