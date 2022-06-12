namespace PackageInstaller.Core.Services;

public class DistributionList
{
    public IReadOnlyList<Distribution> InstalledDistributions { get; init; } =
        Array.Empty<Distribution>();

    public IReadOnlyList<Alert> Alerts { get; init; } = Array.Empty<Alert>();

    public readonly struct Alert
    {
        public string Title { get; init; }
        public string Message { get; init; }
        public AlertPriority Priority { get; init; }
        public string? HelpUrl { get; init; }
    }

    public enum AlertPriority
    {
        /// <summary>
        /// Just an importation for the end user which can safely be ignored.
        /// </summary>
        Information = 0,

        /// <summary>
        /// An important information which indicates the user should intervene.
        /// </summary>
        Important = 2,

        /// <summary>
        /// A critical information and the user must take immediate action to ensure that the product is working as intended.
        /// </summary>
        Critical = 3
    }

    public static DistributionList CreateWithAlertOnly(params Alert[] alerts)
    {
        return new DistributionList() { Alerts = alerts };
    }
}
