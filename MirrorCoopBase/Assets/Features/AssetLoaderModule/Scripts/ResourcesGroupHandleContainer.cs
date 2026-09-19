using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.AssetLoaderModule.Scripts {
    [Serializable]
    public class ResourcesGroupHandleContainer {
        public readonly Dictionary<string, ResourceRequest> CompletedHandles = new();
        public readonly Dictionary<string, List<ResourceRequest>> AllHandles = new();
    }
}
