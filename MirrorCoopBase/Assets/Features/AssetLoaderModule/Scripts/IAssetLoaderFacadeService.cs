using System.Threading.Tasks;
using UnityEngine;

namespace Features.AssetLoaderModule.Scripts {
    public interface IAssetLoaderFacadeService {
        public Task<TAsset> LoadAssetAsync<TAsset>(string key, AssetLoadSource assetLoadSource, string groupName = "Default") where TAsset : Object;
        public TAsset LoadAsset<TAsset>(string key, AssetLoadSource assetLoadSource, string groupName = "Default") where TAsset : Object;
        public void ReleaseAssetsInGroup(AssetLoadSource assetLoadSource, string groupName = "Default");
        public void ReleaseAllAssets(AssetLoadSource assetLoadSource);
        public bool HasLoadedAsset(string key, AssetLoadSource assetLoadSource, string groupName = "Default");
        IAssetLoaderService GetAssetLoaderService(AssetLoadSource assetLoadSource);
    }
}
