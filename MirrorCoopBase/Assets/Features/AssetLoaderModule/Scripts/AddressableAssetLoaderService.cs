using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Features.AssetLoaderModule.Scripts {
    public class AddressableAssetLoaderService : IAddressablesAssetLoaderService {
        protected readonly Dictionary<string, AddressablesGroupHandleContainer> _handlesContainerByGroupName = new();

        public async Task<TAsset> LoadAssetAsync<TAsset>(string key, string groupName = "Default") where TAsset : Object {
            AddressablesGroupHandleContainer handleContainer = GetHandleContainer(groupName);

            return handleContainer.CompletedHandles.TryGetValue(key, out AsyncOperationHandle cachedHandle)
                ? cachedHandle.Result as TAsset
                : await ProcessHandleAsync<TAsset>(key, handleContainer);
        }

        public TAsset LoadAsset<TAsset>(string key, string groupName = "Default") where TAsset : Object {
            AddressablesGroupHandleContainer handleContainer = GetHandleContainer(groupName);

            return handleContainer.CompletedHandles.TryGetValue(key, out AsyncOperationHandle cachedHandle)
                ? cachedHandle.Result as TAsset
                : ProcessHandle<TAsset>(key, handleContainer);
        }

        public void ReleaseAssetsInGroup(string groupName = "Default") {
            if (_handlesContainerByGroupName.TryGetValue(groupName, out AddressablesGroupHandleContainer handleContainer) == false)
                return;

            foreach (KeyValuePair<string, List<AsyncOperationHandle>> allHandlesInContainer in handleContainer.AllHandles)
                foreach (AsyncOperationHandle handle in allHandlesInContainer.Value)
                    Addressables.Release(handle);

            handleContainer.AllHandles.Clear();
            handleContainer.CompletedHandles.Clear();
        }

        public void ReleaseAllAssets() {
            foreach (KeyValuePair<string, AddressablesGroupHandleContainer> handlesContainer in _handlesContainerByGroupName.ToList())
                ReleaseAssetsInGroup(handlesContainer.Key);
        }

        public bool HasLoadedAsset(string key, string groupName = "Default") =>
            _handlesContainerByGroupName.TryGetValue(groupName, out AddressablesGroupHandleContainer handleContainer)
            && handleContainer.CompletedHandles.ContainsKey(key);

        private async Task<TAsset> ProcessHandleAsync<TAsset>(string key, AddressablesGroupHandleContainer handleContainer) where TAsset : Object {
            AsyncOperationHandle<TAsset> handle = Addressables.LoadAssetAsync<TAsset>(key);
            handle.Completed += completedHandle => handleContainer.CompletedHandles[key] = completedHandle;
            AddHandle(key, handle, handleContainer);

            return await handle.Task;
        }

        private TAsset ProcessHandle<TAsset>(string key, AddressablesGroupHandleContainer handleContainer) where TAsset : Object {
            AsyncOperationHandle<TAsset> handle = Addressables.LoadAssetAsync<TAsset>(key);

            handleContainer.CompletedHandles[key] = handle;
            AddHandle(key, handle, handleContainer);
            return handle.WaitForCompletion();
        }

        private void AddHandle<TAsset>(string key, AsyncOperationHandle<TAsset> handle, AddressablesGroupHandleContainer handleContainer) where TAsset : Object {
            if (!handleContainer.AllHandles.TryGetValue(key, out List<AsyncOperationHandle> resourceHandles)) {
                resourceHandles = new List<AsyncOperationHandle>();
                handleContainer.AllHandles[key] = resourceHandles;
            }

            resourceHandles.Add(handle);
        }

        private AddressablesGroupHandleContainer GetHandleContainer(string groupName) {
            if (_handlesContainerByGroupName.TryGetValue(groupName, out AddressablesGroupHandleContainer handleContainer))
                return handleContainer;

            handleContainer = new AddressablesGroupHandleContainer();
            _handlesContainerByGroupName[groupName] = handleContainer;
            return handleContainer;
        }
    }
}
