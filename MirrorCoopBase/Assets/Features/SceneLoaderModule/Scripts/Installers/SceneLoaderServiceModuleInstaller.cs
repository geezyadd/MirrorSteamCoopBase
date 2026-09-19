using Zenject;

namespace Features.SceneLoaderModule.Scripts.Installers {
    public sealed class SceneLoaderServiceModuleInstaller : Installer<SceneLoaderServiceModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesTo<AddressablesSceneLoaderService>().AsSingle();
        }
    }
}
