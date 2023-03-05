using System;
using Microsoft.UI.Dispatching;

namespace Shared.Services.Implementations.WinuiUI;

public class ThreadHelpers : IThreadHelpers
{
    private readonly DispatcherQueue _uiThreadDispatcherQueue;

    public ThreadHelpers(DispatcherQueue uiThreadDispatcherQueue)
    {
        _uiThreadDispatcherQueue = uiThreadDispatcherQueue;

        UiThread = new DispatcherAwaiter(uiThreadDispatcherQueue);
    }

    public bool IsOnUiThread => _uiThreadDispatcherQueue.HasThreadAccess;

    public IThreadHelpers.BaseDispatcherAwaiter UiThread { get; }

    public void Schedule(Action action)
    {
        _uiThreadDispatcherQueue.TryEnqueue(new DispatcherQueueHandler(action));
    }

    class DispatcherAwaiter : IThreadHelpers.BaseDispatcherAwaiter
    {
        private readonly DispatcherQueue _uiThreadDispatcherQueue;

        public DispatcherAwaiter(DispatcherQueue uiThreadDispatcherQueue)
        {
            _uiThreadDispatcherQueue = uiThreadDispatcherQueue;
        }

        public override bool IsCompleted => _uiThreadDispatcherQueue.HasThreadAccess;

        public override void OnCompleted(Action continuation) =>
            _uiThreadDispatcherQueue.TryEnqueue(() => continuation());

        public override void GetResult() { }

        public override DispatcherAwaiter GetAwaiter()
        {
            return this;
        }
    }
}
