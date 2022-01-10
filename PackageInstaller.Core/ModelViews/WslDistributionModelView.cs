using System.Reactive.Linq;
using PackageInstaller.Core.Services;
using ReactiveUI;

namespace PackageInstaller.Core.ModelViews
{
    public class WslDistributionModelView : ReactiveObject
    {
        public WslDistributionModelView(WslDistribution distribution)
        {
            _name = this.WhenAnyValue((x) => x.Distro, (WslDistribution d) => d.Name)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, (x) => x.Name);

            _icon = this.WhenAnyValue((x) => x.Distro, (WslDistribution x) => x.Type)
                .Select(GetIconUriFromType)
                .ToProperty(this, (x) => x.Icon);

            Distro = distribution;
        }

        private string GetIconUriFromType(WslDistributionType type)
        {
            switch (type)
            {
                case WslDistributionType.Ubuntu:
                    return ToUri("128_ubuntu.png");
                case WslDistributionType.Debian:
                    return ToUri("128_debian.png");
                case WslDistributionType.Fedora:
                    return ToUri("128_debian.png");
                case WslDistributionType.Arch:
                    return ToUri("128_fedora_newlogo.png");
                case WslDistributionType.Kali:
                    return ToUri("128_kali.png");
                case WslDistributionType.Alpine:
                    return ToUri("128_alpine.png");
                case WslDistributionType.OpenSUSE:
                    return ToUri("128_suse.png");
                case WslDistributionType.SUSE:
                    return ToUri("128_suse.png");
                case WslDistributionType.GeneralDebBased:
                    return ToUri("128_unknown.png");
                case WslDistributionType.GeneralRpmBased:
                    return ToUri("128_unknown.png");
                case WslDistributionType.Unknown:
                    return ToUri("128_unknown.png");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            string ToUri(string name)
            {
                return $"Assets/DistroIcons/{name}";
            }
        }

        WslDistribution _distro;

        public WslDistribution Distro
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
}
