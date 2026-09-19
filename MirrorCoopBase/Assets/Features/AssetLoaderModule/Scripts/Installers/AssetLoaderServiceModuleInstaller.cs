using Zenject;

namespace Features.AssetLoaderModule.Scripts.Installers {
    public sealed class AssetLoaderServiceModuleInstaller : Installer<AssetLoaderServiceModuleInstaller> {
        public override void InstallBindings() {
            Container.Bind<IResourceAssetLoaderService>()
                .To<ResourceAssetLoaderService>()
                .AsSingle();

            Container.Bind<IAddressablesAssetLoaderService>()
                .To<AddressableAssetLoaderService>()
                .AsSingle();

            Container.BindInterfacesTo<AssetLoaderFacadeService>()
                .AsSingle();
        }
    }
}
