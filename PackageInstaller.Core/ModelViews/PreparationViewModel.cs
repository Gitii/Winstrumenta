﻿using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using PackageInstaller.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;
using Shared.Services;

namespace PackageInstaller.Core.ModelViews;

public class PreparationViewModel : ReactiveObject, IViewModel, INavigable
{
    private readonly IEnumerable<IPlatformDependentPackageManager> _packagesManagers;
    private readonly IPath _path;
    private readonly IIconThemeManager _iconThemeManager;
    private readonly IFile _file;
    private readonly IDisposableFiles _disposableFiles;

    public readonly struct NavigationParameter
    {
        public string[] Arguments { get; init; }
    }

    private readonly IParameterViewStackService _viewStackService;
    public string Id { get; } = nameof(PreparationViewModel);

    public PreparationViewModel(
        IParameterViewStackService viewStackService,
        IEnumerable<IPlatformDependentPackageManager> packagesManagers,
        IPath path,
        IIconThemeManager iconThemeManager,
        IFile file,
        IDisposableFiles disposableFiles
    )
    {
        _viewStackService = viewStackService;
        _packagesManagers = packagesManagers;
        _path = path;
        _iconThemeManager = iconThemeManager;
        _file = file;
        _disposableFiles = disposableFiles;
    }

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
        var navParms = parameter.FromNavigationParameter<NavigationParameter>();

        return ProcessAsync(navParms.Arguments).ToObservable(RxApp.MainThreadScheduler);
    }

    public async Task ProcessAsync(string[] arguments)
    {
        await Task.Delay(10).ConfigureAwait(true);

        try
        {
            if (arguments.Length == 0)
            {
                NavigateToGettingStarted();

                return;
            }

            await _iconThemeManager.LoadThemesAsync().ConfigureAwait(true);

            var packageFilePath = ParseArguments(arguments);

            var packageManager = await _packagesManagers
                .GetSupportedManagerAsync(packageFilePath)
                .ConfigureAwait(true);

            var data = await packageManager
                .ExtractPackageMetaDataAsync(packageFilePath)
                .ConfigureAwait(true);

            var navParms = new PackageActionsViewModel.NavigationParameter()
            {
                PackageFilePath = packageFilePath,
                PackageMetaData = data
            };

            _viewStackService
                .PushPage<PackageActionsViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
        catch (Exception e)
        {
            var navParms = new ErrorViewModel.NavigationParameter() { Exception = e };

            _viewStackService
                .PushPage<ErrorViewModel>(navParms.ToNavigationParameter())
                .Subscribe();
        }
    }

    private void NavigateToGettingStarted()
    {
        _viewStackService.PushPage<GettingStartedModelView>().Subscribe();
    }

    private FileSystemPath ParseArguments(string[] arguments)
    {
        if (arguments.Length > 1)
        {
            throw new Exception(
                "Too many arguments passed in. The first argument must be the file path to the debian package."
            );
        }

        var filePath = arguments[0];

        if (!_file.Exists(filePath))
        {
            throw new FileNotFoundException("The file doesn't exist", filePath);
        }

        var fullPath = _path.GetFullPath(filePath);

        if (_path.IsWslNetworkShare(fullPath) || _path.IsNetworkShare(fullPath))
        {
            // copy the original file to a temp location if it's on a network path
            var tempFile = _file.GetTemporaryFilePath(Path.GetExtension(fullPath));
            _disposableFiles.AddFiles(tempFile);

            _file.CopyFile(fullPath, tempFile);

            filePath = tempFile;
        }

        return _path.ToFileSystemPath(filePath);
    }
}
