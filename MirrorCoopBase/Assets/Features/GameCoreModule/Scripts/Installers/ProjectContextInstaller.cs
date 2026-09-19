using Features.AssetLoaderModule.Scripts.Installers;
using Features.SceneLoaderModule.Scripts.Installers;
using Zenject;

namespace Features.GameCoreModule.Scripts.Installers {
    public sealed class ProjectContextInstaller : MonoInstaller<ProjectContextInstaller> {
        public override void InstallBindings() {
            DataInstaller.Install(Container);
            AssetLoaderServiceModuleInstaller.Install(Container);
            ConfigurationInstaller.Install(Container);
            SceneLoaderServiceModuleInstaller.Install(Container);
        }
    }
}
