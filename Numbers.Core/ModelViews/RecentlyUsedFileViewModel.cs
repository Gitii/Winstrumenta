using System.Reactive;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;

namespace Numbers.Core.ModelViews;

public class RecentlyUsedFileViewModel : ReactiveObject, IViewModel
{
    private readonly IParameterViewStackService _viewStackService;

    public string Id { get; } = nameof(RecentlyUsedFileViewModel);

    public RecentlyUsedFileViewModel(IParameterViewStackService viewStackService)
    {
        _viewStackService = viewStackService;

        OpenDocument = ReactiveCommand.Create(Open);
        _fileName = _path = _formatedAccessDate = String.Empty;
    }

    private void Open()
    {
        var navParams = new PreparationViewModel.NavigationParameter()
        {
            Arguments = new[] { Path },
        };

        _viewStackService
            .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
            .Subscribe();
    }

    public RecentlyUsedFileViewModel Set(RecentlyUsedFile file)
    {
        FileName = file.FileName;
        Path = file.FullPath;
        FormatedAccessDate = file.AccessTime.ToShortDateString();

        return this;
    }

    string _fileName;
    public string FileName
    {
        get { return _fileName; }
        private set { this.RaiseAndSetIfChanged(ref _fileName, value); }
    }

    string _formatedAccessDate;
    public string FormatedAccessDate
    {
        get { return _formatedAccessDate; }
        private set { this.RaiseAndSetIfChanged(ref _formatedAccessDate, value); }
    }

    string _path;
    public string Path
    {
        get { return _path; }
        private set { this.RaiseAndSetIfChanged(ref _path, value); }
    }

    public ReactiveCommand<Unit, Unit> OpenDocument { get; }
}
