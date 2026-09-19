using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Features.AssetLoaderModule.Scripts {
    public class ResourceAssetLoaderService : IResourceAssetLoaderService {
        private const float TIME_OUT_THRESHOLD = 10f;
        protected readonly Dictionary<string, ResourcesGroupHandleContainer> _handlesContainerByGroupName = new();

        public async Task<TAsset> LoadAssetAsync<TAsset>(string key, string groupName = "Default") where TAsset : Object {
            ResourcesGroupHandleContainer handleContainer = GetHandleContainer(groupName);

            return handleContainer.CompletedHandles.TryGetValue(key, out ResourceRequest cachedHandle)
                ? cachedHandle.asset as TAsset
                : await ProcessHandleAsync<TAsset>(key, handleContainer);
        }

        public TAsset LoadAsset<TAsset>(string key, string groupName = "Default") where TAsset : Object {
            ResourcesGroupHandleContainer handleContainer = GetHandleContainer(groupName);

            return handleContainer.CompletedHandles.TryGetValue(key, out ResourceRequest cachedHandle)
                ? cachedHandle.asset as TAsset
                : ProcessHandle<TAsset>(key, handleContainer);
        }

        public void ReleaseAssetsInGroup(string groupName = "Default") {
            if (_handlesContainerByGroupName.ContainsKey(groupName) == false)
                return;

            _handlesContainerByGroupName[groupName] = new ResourcesGroupHandleContainer();
        }

        public void ReleaseAllAssets() {
            foreach (var handlesContainer in _handlesContainerByGroupName.ToList())
                ReleaseAssetsInGroup(handlesContainer.Key);
        }

        public bool HasLoadedAsset(string key, string groupName = "Default") =>
            _handlesContainerByGroupName.TryGetValue(groupName, out ResourcesGroupHandleContainer handleContainer)
            && handleContainer.CompletedHandles.ContainsKey(key);

        private async Task<TAsset> ProcessHandleAsync<TAsset>(string key, ResourcesGroupHandleContainer handleContainer) where TAsset : Object {
            ResourceRequest handle = Resources.LoadAsync<TAsset>(key);
            handle.completed += _ => handleContainer.CompletedHandles[key] = handle;
            AddHandle(key, handle, handleContainer);

            while (handle.isDone == false)
                await Task.Yield();

            return handle.asset as TAsset;
        }

        private void AddHandle(string key, ResourceRequest handle, ResourcesGroupHandleContainer handleContainer) {
            if (!handleContainer.AllHandles.TryGetValue(key, out List<ResourceRequest> resourceHandles)) {
                resourceHandles = new List<ResourceRequest>();
                handleContainer.AllHandles[key] = resourceHandles;
            }

            resourceHandles.Add(handle);
        }

        private bool ProcessTimeOut(ResourceRequest handle) {
            DateTime dateTimeToBreakLoop = DateTime.Now.AddSeconds(TIME_OUT_THRESHOLD);
            while (handle.asset == null) {
                if (DateTime.Now > dateTimeToBreakLoop)
                    return true;
            }

            return false;
        }

        private TAsset ProcessHandle<TAsset>(string key, ResourcesGroupHandleContainer handleContainer) where TAsset : Object {
            ResourceRequest handle = Resources.LoadAsync<TAsset>(key);
            if (ProcessTimeOut(handle))
                throw new TimeoutException($"Can't load asset from resources by key {key}.");

            handleContainer.CompletedHandles[key] = handle;
            AddHandle(key, handle, handleContainer);
            return handle.asset as TAsset;
        }

        private ResourcesGroupHandleContainer GetHandleContainer(string groupName) {
            if (_handlesContainerByGroupName.TryGetValue(groupName, out ResourcesGroupHandleContainer handleContainer))
                return handleContainer;

            handleContainer = new ResourcesGroupHandleContainer();
            _handlesContainerByGroupName[groupName] = handleContainer;
            return handleContainer;
        }
    }
}
