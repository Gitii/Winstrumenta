using System.Runtime.CompilerServices;

namespace PackageInstaller.Core.Services;

public interface IThreadHelpers
{
    bool IsOnUiThread { get; }

    BaseDispatcherAwaiter UiThread { get; }

    public abstract class BaseDispatcherAwaiter : INotifyCompletion
    {
        public abstract bool IsCompleted { get; }

        public abstract void OnCompleted(Action continuation);

        public abstract void GetResult();

        public abstract BaseDispatcherAwaiter GetAwaiter();
    }

    void Schedule(Action action);
}
