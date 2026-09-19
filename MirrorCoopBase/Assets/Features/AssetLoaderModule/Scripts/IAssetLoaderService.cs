using System.Threading.Tasks;
using UnityEngine;

namespace Features.AssetLoaderModule.Scripts {
    public interface IAssetLoaderService {
        public Task<TAsset> LoadAssetAsync<TAsset>(string key, string groupName = "Default") where TAsset : Object;
        public TAsset LoadAsset<TAsset>(string key, string groupName = "Default") where TAsset : Object;
        public void ReleaseAssetsInGroup(string groupName = "Default");
        public void ReleaseAllAssets();
        public bool HasLoadedAsset(string key, string groupName = "Default");
    }
}
