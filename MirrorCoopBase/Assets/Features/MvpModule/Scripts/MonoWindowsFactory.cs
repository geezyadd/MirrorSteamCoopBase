using System;
using System.Threading.Tasks;
using Features.AssetLoaderModule.Scripts;
using UnityEngine;
using Zenject;

namespace Features.MvpModule {
    public class MonoWindowsFactory : IWindowsFactory {
        private const string ROOT_KEY = "WindowInstances/";
        private const string DEFAULT_GROUP_NAME = "Default";
        private readonly IAssetLoaderFacadeService _assetLoaderFacadeService;
        private readonly PreloadedWindowsModel _preloadedWindowsModel;
        private readonly WindowsRootService _windowsRoot;
        private readonly DiContainer _container;

        public MonoWindowsFactory(
            IAssetLoaderFacadeService assetLoaderFacadeService,
            DiContainer container,
            PreloadedWindowsModel preloadedWindowsModel,
            WindowsRootService windowsRoot) {
            _container = container;
            _windowsRoot = windowsRoot ?? throw new ArgumentNullException(nameof(windowsRoot));
            _assetLoaderFacadeService = assetLoaderFacadeService ?? throw new ArgumentNullException(nameof(assetLoaderFacadeService));
            _preloadedWindowsModel = preloadedWindowsModel ?? throw new ArgumentNullException(nameof(preloadedWindowsModel));
        }

        public IWindowInstance GetWindowInstanceForWindowType(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName = DEFAULT_GROUP_NAME) {
            return InstantiateWindow(LoadWindowPrefab(windowType, assetLoadSource, assetGroupName), windowType, assetLoadSource);
        }

        public async Task<IWindowInstance> GetWindowInstanceForWindowTypeAsync(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName = DEFAULT_GROUP_NAME) {
            return InstantiateWindow(await LoadWindowPrefabAsync(windowType, assetLoadSource, assetGroupName), windowType, assetLoadSource);
        }
        
        public IWindowInstance GetPreloadWindowInstanceForWindowType(Type windowType) {
            if (!_preloadedWindowsModel.PreloadedWindows.TryGetValue(windowType.Name, out MonoWindowInstance monoWindowInstance))
                throw new WindowPrefabCouldNotBeLoadedException(windowType, ROOT_KEY + windowType);
        
            return InstantiateWindow(monoWindowInstance, windowType, AssetLoadSource.Addressables);
        }

        IWindowInstance InstantiateWindow(MonoWindowInstance prefab, Type windowType, AssetLoadSource assetLoadSource) {
            if (prefab == null)
                throw new WindowPrefabCouldNotBeLoadedException(windowType, ROOT_KEY + windowType, assetLoadSource);

            GameObject instance = _container.InstantiatePrefab(prefab.gameObject, _windowsRoot.Root);
            return instance.GetComponent<MonoWindowInstance>();
        }

        MonoWindowInstance LoadWindowPrefab(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName) {
            try {
                return DownloadWindowInstance(windowType, assetLoadSource, assetGroupName);
            }
            catch (Exception innerException) {
                throw new WindowPrefabCouldNotBeLoadedException(windowType, ROOT_KEY + windowType, assetLoadSource, innerException);
            }
        }

        async Task<MonoWindowInstance> LoadWindowPrefabAsync(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName) {
            try {
                return await DownloadWindowInstanceAsync(windowType, assetLoadSource, assetGroupName);
            }
            catch (Exception innerException) {
                throw new WindowPrefabCouldNotBeLoadedException(windowType, ROOT_KEY + windowType, assetLoadSource, innerException);
            }
        }

        private MonoWindowInstance DownloadWindowInstance(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName) =>
            assetLoadSource switch {
                AssetLoadSource.Addressables => DownloadInstanceFromAddressables(windowType, assetGroupName),
                AssetLoadSource.Resources => DownloadInstanceFromResources(windowType, assetGroupName),
                _ => throw new ArgumentOutOfRangeException(nameof(assetLoadSource), assetLoadSource, null)
            };
        
        private async Task<MonoWindowInstance> DownloadWindowInstanceAsync(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName) =>
            assetLoadSource switch {
                AssetLoadSource.Addressables => await DownloadInstanceFromAddressablesAsync(windowType, assetGroupName),
                AssetLoadSource.Resources => await DownloadInstanceFromResourcesAsync(windowType, assetGroupName),
                _ => throw new ArgumentOutOfRangeException(nameof(assetLoadSource), assetLoadSource, null)
            };

        private MonoWindowInstance DownloadInstanceFromAddressables(Type windowType, string assetGroupName) =>
            _assetLoaderFacadeService.GetAssetLoaderService(AssetLoadSource.Addressables)
                .LoadAsset<GameObject>(windowType.Name, assetGroupName).GetComponent<MonoWindowInstance>();

        private MonoWindowInstance DownloadInstanceFromResources(Type windowType, string assetGroupName) =>
            _assetLoaderFacadeService.GetAssetLoaderService(AssetLoadSource.Resources)
                .LoadAsset<MonoWindowInstance>(ROOT_KEY + windowType.Name, assetGroupName);
        
        private async Task<MonoWindowInstance> DownloadInstanceFromAddressablesAsync(Type windowType, string assetGroupName) =>
            (await _assetLoaderFacadeService.GetAssetLoaderService(AssetLoadSource.Addressables)
                .LoadAssetAsync<GameObject>(windowType.Name, assetGroupName)).GetComponent<MonoWindowInstance>();

        private async Task<MonoWindowInstance> DownloadInstanceFromResourcesAsync(Type windowType, string assetGroupName) =>
            await _assetLoaderFacadeService.GetAssetLoaderService(AssetLoadSource.Resources)
                .LoadAssetAsync<MonoWindowInstance>(ROOT_KEY + windowType.Name, assetGroupName);
    }
}