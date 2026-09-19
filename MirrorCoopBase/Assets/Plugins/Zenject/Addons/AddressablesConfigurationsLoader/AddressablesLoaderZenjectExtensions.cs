using System.Threading.Tasks;
using Features.AssetLoaderModule.Scripts;
using UnityEngine;
using Zenject;

namespace Features.Zenject.Zenject.Addons.AddressablesConfigurationsLoader {
    public static class AddressablesLoaderZenjectExtensions {
        public static ScopeConcreteIdArgConditionCopyNonLazyBinder BindConfigurationFromAddressables<T>(this DiContainer container, string addressableKey) where T : ScriptableObject {
            T configuration = container.Resolve<IAssetLoaderFacadeService>().LoadAsset<T>(addressableKey, AssetLoadSource.Addressables);

            return container.Bind<T>().FromScriptableObject(configuration);
        }

        public static ScopeConcreteIdArgConditionCopyNonLazyBinder BindConfigurationFromAddressables<T>(this DiContainer container, string addressableKey, IAssetLoaderFacadeService assetLoaderFacadeService) where T : ScriptableObject {
            T configuration = assetLoaderFacadeService.LoadAsset<T>(addressableKey, AssetLoadSource.Addressables);

            return container.Bind<T>().FromScriptableObject(configuration);
        }

        public static async Task<ScopeConcreteIdArgConditionCopyNonLazyBinder> BindConfigurationFromAddressablesAsync<T>(this DiContainer container, string addressableKey) where T : ScriptableObject {
            T configuration = await container.Resolve<IAssetLoaderFacadeService>().LoadAssetAsync<T>(addressableKey, AssetLoadSource.Addressables);

            return container.Bind<T>().FromScriptableObject(configuration);
        }

        public static async Task<ScopeConcreteIdArgConditionCopyNonLazyBinder> BindConfigurationFromAddressablesAsync<T>(this DiContainer container, string addressableKey, IAssetLoaderFacadeService assetLoaderFacadeService) where T : ScriptableObject {
            T configuration = await assetLoaderFacadeService.LoadAssetAsync<T>(addressableKey, AssetLoadSource.Addressables);

            return container.Bind<T>().FromScriptableObject(configuration);
        }

        public static ScopeConcreteIdArgConditionCopyNonLazyBinder BindConfigurationFromAddressablesToInterface<TConfiguration, TInterface>(this DiContainer container, string addressableKey) where TConfiguration : ScriptableObject, TInterface {
            TConfiguration configuration = container.Resolve<IAssetLoaderFacadeService>().LoadAsset<TConfiguration>(addressableKey, AssetLoadSource.Addressables);

            return container.Bind<TInterface>().To<TConfiguration>().FromScriptableObject(configuration);
        }
    }
}
