using PackageInstaller.Core.Services;
using ReactiveUI;

namespace PackageInstaller.Core.ModelViews;

public class NotificationModelView : ReactiveObject
{
    Uri? _hyperlink = null;
    string _message = "";
    DistributionList.AlertPriority _priority = DistributionList.AlertPriority.Important;
    string _title = "";

    public NotificationModelView(DistributionList.Alert alert)
    {
        Title = alert.Title;
        Message = alert.Message;
        Priority = alert.Priority;
        Hyperlink = alert.HelpUrl != null ? new Uri(alert.HelpUrl) : null;
    }

    public string Title
    {
        get { return _title; }
        set { this.RaiseAndSetIfChanged(ref _title, value); }
    }

    public string Message
    {
        get { return _message; }
        set { this.RaiseAndSetIfChanged(ref _message, value); }
    }

    public DistributionList.AlertPriority Priority
    {
        get { return _priority; }
        set { this.RaiseAndSetIfChanged(ref _priority, value); }
    }

    public Uri? Hyperlink
    {
        get { return _hyperlink; }
        set { this.RaiseAndSetIfChanged(ref _hyperlink, value); }
    }
}
