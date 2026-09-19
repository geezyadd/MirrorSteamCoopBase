using System.Threading.Tasks;
using UnityEngine;

namespace Features.AssetLoaderModule.Scripts {
    public interface IResourceAssetLoaderService : IAssetLoaderService {
        public new Task<TAsset> LoadAssetAsync<TAsset>(string key, string groupName = "Default") where TAsset : Object;
        public new TAsset LoadAsset<TAsset>(string key, string groupName = "Default") where TAsset : Object;
        public new void ReleaseAssetsInGroup(string groupName = "Default");
        public new void ReleaseAllAssets();
        public new bool HasLoadedAsset(string key, string groupName = "Default");
    }
}
