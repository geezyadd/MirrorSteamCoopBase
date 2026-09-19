using System;
using Features.AssetLoaderModule.Scripts;

namespace Features.MvpModule {
    public class WindowPrefabCouldNotBeLoadedException : Exception {
        public WindowPrefabCouldNotBeLoadedException(Type windowType, string key, AssetLoadSource source) 
            : base(GenerateMessage(windowType, key, source.ToString())) { }
        public WindowPrefabCouldNotBeLoadedException(Type windowType, string key, AssetLoadSource source, Exception innerException)
            : base(GenerateMessage(windowType, key, source.ToString()) + $"\n Exception text: {innerException}") { }
        public WindowPrefabCouldNotBeLoadedException(Type windowType, string key) : base(GenerateMessage(windowType, key, string.Empty)) { }
        
        private static string GenerateMessage(Type windowType, string key, string loadSource) =>
            $"Window of type {windowType} coud not be loaded " +
            $"from {loadSource}. Make sure you have been added window prefab to " +
            $"{loadSource} folder by key {key} and added MonoWindowInstance component to this prefab";
    }
}