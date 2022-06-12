namespace PackageInstaller.Core.Services;

#if DEBUG
public class DummyProvider : IDistributionProvider
{
    public async Task<DistributionList> GetAllInstalledDistributionsAsync(
        string packageExtensionHint
    )
    {
        return new DistributionList()
        {
            Alerts = new List<DistributionList.Alert>()
            {
                new DistributionList.Alert()
                {
                    Title = "Title",
                    Message = "Message",
                    Priority = DistributionList.AlertPriority.Critical,
                    HelpUrl =
                        "https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable.maxby?view=net-6.0"
                },
                new DistributionList.Alert()
                {
                    Title = "Title2",
                    Message = "Message2",
                    Priority = DistributionList.AlertPriority.Important,
                    HelpUrl = "https://www.google.de"
                },
                new DistributionList.Alert()
                {
                    Title = "Title3",
                    Message = "Message3",
                    Priority = DistributionList.AlertPriority.Information,
                    HelpUrl =
                        "https://docs.microsoft.com/en-us/dotnet/api/system.argumentnullexception?view=net-6.0"
                }
            }
        };
    }
}
#endif
