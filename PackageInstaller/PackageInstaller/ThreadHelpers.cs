using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using PackageInstaller.Core.Services;

namespace PackageInstaller;

class ThreadHelpers : IThreadHelpers
{
    private readonly DispatcherQueue _uiThreadDispatcherQueue;

    public ThreadHelpers(DispatcherQueue uiThreadDispatcherQueue)
    {
        _uiThreadDispatcherQueue = uiThreadDispatcherQueue;

        UiThread = new DispatcherAwaiter(uiThreadDispatcherQueue);
    }

    public bool IsOnUiThread => _uiThreadDispatcherQueue.HasThreadAccess;

    public IThreadHelpers.BaseDispatcherAwaiter UiThread { get; }

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
