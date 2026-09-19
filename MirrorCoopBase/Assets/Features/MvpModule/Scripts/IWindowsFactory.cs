using System;
using System.Threading.Tasks;
using Features.AssetLoaderModule.Scripts;

namespace Features.MvpModule {
    public interface IWindowsFactory {
        public IWindowInstance GetWindowInstanceForWindowType(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName = "Default");
        public Task<IWindowInstance> GetWindowInstanceForWindowTypeAsync(Type windowType, AssetLoadSource assetLoadSource, string assetGroupName = "Default");
        public IWindowInstance GetPreloadWindowInstanceForWindowType(Type windowType);
    }
}