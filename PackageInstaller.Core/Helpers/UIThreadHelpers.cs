using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace PackageInstaller.Core.Helpers;

public static class UIThreadHelpers
{
    public static void AssertIsOnUiThread(this object _)
    {
        if (RxApp.MainThreadScheduler != CurrentThreadScheduler.Instance)
        {
            throw new Exception("The current thread is not the UI thread!");
        }
    }
}
