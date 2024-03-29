﻿using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

namespace PackageInstaller.Core.ModelViews;

public class GettingStartedModelView : ReactiveObject, IViewModel, INavigable
{
    private readonly ILauncher _launcher;
    private readonly Interaction<Unit, PickFileOutput> _pickFileInteraction;
    private readonly IParameterViewStackService _viewStackService;
    private readonly IKnownFolders _knownFolders;
    private readonly IApplicationLifeCycle _lifeCycle;

    public GettingStartedModelView(
        ILauncher launcher,
        IParameterViewStackService viewStackService,
        IKnownFolders knownFolders,
        IApplicationLifeCycle lifeCycle
    )
    {
        _launcher = launcher;
        _viewStackService = viewStackService;
        _knownFolders = knownFolders;
        _lifeCycle = lifeCycle;
        _pickFileInteraction = new Interaction<Unit, PickFileOutput>(RxApp.MainThreadScheduler);

        PickFilesCommand = ReactiveCommand.CreateFromTask(PickFilesAsync);
        LaunchExplorerCommand = ReactiveCommand.CreateFromTask(LaunchExplorerAsync);
        LaunchWithFileCommand = ReactiveCommand.CreateFromTask<string?>(LaunchWithFileAsync);
        ExitCommand = ReactiveCommand.Create(() => _lifeCycle.Exit(0));
    }

    private async Task LaunchWithFileAsync(string? filePath)
    {
        if (filePath != null)
        {
            var navParams = new PreparationViewModel.NavigationParameter()
            {
                Arguments = new[] { filePath }
            };

            _viewStackService
                .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
                .Subscribe();
        }
    }

    private Task LaunchExplorerAsync()
    {
        return _launcher.LaunchFolderAsync(_knownFolders.GetPath(KnownFolder.Downloads));
    }

    private async Task PickFilesAsync()
    {
        var output = await PickFileInteraction.Handle(Unit.Default);

        if (output.PickedFilePath != null)
        {
            var navParams = new PreparationViewModel.NavigationParameter()
            {
                Arguments = new[] { output.PickedFilePath }
            };

            _viewStackService
                .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
                .Subscribe();
        }
    }

    public ReactiveCommand<Unit, Unit> PickFilesCommand { get; }
    public ReactiveCommand<Unit, Unit> LaunchExplorerCommand { get; }
    public ReactiveCommand<string?, Unit> LaunchWithFileCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    public string Id { get; } = nameof(GettingStartedModelView);

    public Interaction<Unit, PickFileOutput> PickFileInteraction => _pickFileInteraction;

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
        return Observable.Return(Unit.Default);
    }
}
