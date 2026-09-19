using Features.AddressablesConstantsGenerator.Generated;
using Features.Zenject.Zenject.Addons.AddressablesConfigurationsLoader;
using Game.Connection;
using Zenject;

namespace Features.GameCoreModule.Scripts.Installers {
    public sealed class ConfigurationInstaller : Installer<ConfigurationInstaller> {
        public override void InstallBindings() {
            Container.BindConfigurationFromAddressables<ConnectionConfig>(Address.Configurations.ConnectionConfig_Default)
                .AsSingle();
        }
    }
}
