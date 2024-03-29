﻿using System.Reactive.Concurrency;
using ReactiveUI;

namespace Shared.Misc;

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
