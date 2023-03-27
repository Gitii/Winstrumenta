using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Numbers.Core.Services;
using ReactiveUI;
using Sextant;
using Shared.Misc;

namespace Numbers.Core.Services { }

namespace Numbers.Core.ModelViews
{
    public class StartViewModel : ReactiveObject, IViewModel, INavigable
    {
        public string Id { get; } = nameof(StartViewModel);

        private readonly IExplorerShell _explorerShellService;
        private readonly IParameterViewStackService _viewStackService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Interaction<Unit, string?> _pickCsvDocumentInteraction;

        public StartViewModel(
            IExplorerShell explorerShellService,
            IParameterViewStackService viewStackService,
            IServiceProvider serviceProvider
        )
        {
            _explorerShellService = explorerShellService;
            _viewStackService = viewStackService;
            _serviceProvider = serviceProvider;
            _recentlyUsedFiles = ImmutableList<RecentlyUsedFileViewModel>.Empty;

            OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
            _pickCsvDocumentInteraction = new Interaction<Unit, string?>(RxApp.MainThreadScheduler);
        }

        private async Task OpenFileAsync()
        {
            var filePath = await _pickCsvDocumentInteraction
                .Handle(Unit.Default)
                .ToTask()
                .ConfigureAwait(true);

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var navParams = new PreparationViewModel.NavigationParameter()
            {
                Arguments = new[] { filePath },
            };

            _viewStackService
                .PushPage<PreparationViewModel>(navParams.ToNavigationParameter())
                .Subscribe();
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
            return ProcessAsync().ToObservable(RxApp.MainThreadScheduler);
        }

        ImmutableList<RecentlyUsedFileViewModel> _recentlyUsedFiles;

        public ImmutableList<RecentlyUsedFileViewModel> RecentlyUsedFiles
        {
            get { return _recentlyUsedFiles; }
            private set { this.RaiseAndSetIfChanged(ref _recentlyUsedFiles, value); }
        }

        public async Task ProcessAsync()
        {
            await Task.Delay(10).ConfigureAwait(true);

            RecentlyUsedFiles = _explorerShellService
                .GetRecentlyUsedFiles(".csv")
                .OrderByDescending((i) => i.AccessTime)
                .Select(
                    (i) => _serviceProvider.GetRequiredService<RecentlyUsedFileViewModel>().Set(i)
                )
                .ToImmutableList();
        }

        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

        public Interaction<Unit, string?> PickCsvDocumentInteraction => _pickCsvDocumentInteraction;
    }
}
