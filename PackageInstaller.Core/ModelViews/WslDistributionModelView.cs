using System.Reactive.Linq;
using PackageInstaller.Core.Services;
using ReactiveUI;

namespace PackageInstaller.Core.ModelViews;

public class WslDistributionModelView : ReactiveObject
{
    public WslDistributionModelView(Distribution distribution)
    {
        _name = this.WhenAnyValue((x) => x.Distro, (Distribution d) => d.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, (x) => x.Name);

        _icon = this.WhenAnyValue((x) => x.Distro, (Distribution x) => x.Type)
            .Select(GetIconUriFromType)
            .ToProperty(this, (x) => x.Icon);

        Distro = distribution;
    }

    private string GetIconUriFromType(DistributionType type)
    {
        switch (type)
        {
            case DistributionType.Ubuntu:
                return ToUri("128_ubuntu.png");
            case DistributionType.Debian:
                return ToUri("128_debian.png");
            case DistributionType.Fedora:
                return ToUri("128_debian.png");
            case DistributionType.Arch:
                return ToUri("128_fedora_newlogo.png");
            case DistributionType.Kali:
                return ToUri("128_kali.png");
            case DistributionType.Alpine:
                return ToUri("128_alpine.png");
            case DistributionType.OpenSUSE:
                return ToUri("128_suse.png");
            case DistributionType.SUSE:
                return ToUri("128_suse.png");
            case DistributionType.Android:
                return ToUri("128_android.png");
            case DistributionType.GeneralDebBased:
                return ToUri("128_unknown.png");
            case DistributionType.GeneralRpmBased:
                return ToUri("128_unknown.png");
            case DistributionType.Unknown:
                return ToUri("128_unknown.png");
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        string ToUri(string name)
        {
            return $"Assets/DistroIcons/{name}";
        }
    }

    Distribution _distro;

    public Distribution Distro
    {
        get { return _distro; }
        set { this.RaiseAndSetIfChanged(ref _distro, value); }
    }

    readonly ObservableAsPropertyHelper<string> _name;
    public string Name => _name.Value;

    readonly ObservableAsPropertyHelper<string> _icon;
    public string Icon => _icon.Value;

    public override string ToString()
    {
        return $"Distro {Name} ({Distro.Version}, {Distro.Type})";
    }
}
