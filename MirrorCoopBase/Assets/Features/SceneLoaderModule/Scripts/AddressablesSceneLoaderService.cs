using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Features.SceneLoaderModule.Scripts {
    public sealed class AddressablesSceneLoaderService : ISceneLoaderService {
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedScenes = new();

        public async Task LoadSceneAsync(string sceneToLoad, bool unloadRedundant) {
            if (_loadedScenes.TryGetValue(sceneToLoad, out AsyncOperationHandle<SceneInstance> existingHandle)) {
                if (existingHandle.IsValid() && existingHandle.IsDone == false)
                    await existingHandle.Task;
                return;
            }

            if (unloadRedundant)
                await UnloadScenesAsync(_loadedScenes.Keys.ToList());

            LoadSceneMode loadSceneMode = unloadRedundant
                ? LoadSceneMode.Single
                : LoadSceneMode.Additive;

            AsyncOperationHandle<SceneInstance> handle =
                Addressables.LoadSceneAsync(sceneToLoad, loadSceneMode);
            _loadedScenes[sceneToLoad] = handle;
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded) {
                _loadedScenes.Remove(sceneToLoad);
                Debug.LogError($"Failed to load addressable scene '{sceneToLoad}'.");
            }
        }

        public async Task LoadScenesAsync(List<string> scenesToLoad, string activeScene, bool unloadRedundant) {
            if (unloadRedundant)
                await UnloadScenesAsync(_loadedScenes.Keys.Except(scenesToLoad).ToList());

            foreach (string sceneName in scenesToLoad)
                await LoadSceneAsync(sceneName, false);

            Scene scene = SceneManager.GetSceneByName(activeScene);
            if (scene.IsValid() && scene.isLoaded)
                SceneManager.SetActiveScene(scene);
        }

        public async Task UnloadSceneAsync(string sceneToUnload) {
            ActivateFallbackIfActive(sceneToUnload);

            if (_loadedScenes.TryGetValue(sceneToUnload, out AsyncOperationHandle<SceneInstance> handle)) {
                _loadedScenes.Remove(sceneToUnload);
                if (handle.IsValid()) {
                    AsyncOperationHandle unloadHandle = Addressables.UnloadSceneAsync(
                        handle,
                        UnloadSceneOptions.UnloadAllEmbeddedSceneObjects,
                        false);
                    await unloadHandle.Task;

                    bool unloadFailed = unloadHandle.IsValid() &&
                                        unloadHandle.Status != AsyncOperationStatus.Succeeded;
                    if (unloadHandle.IsValid())
                        Addressables.Release(unloadHandle);

                    if (unloadFailed)
                        Debug.LogError($"Failed to unload addressable scene '{sceneToUnload}'.");
                }
            }

            Scene leftover = SceneManager.GetSceneByName(sceneToUnload);
            if (leftover.IsValid() == false || leftover.isLoaded == false)
                return;

            ActivateFallbackIfActive(sceneToUnload);
            AsyncOperation operation = SceneManager.UnloadSceneAsync(leftover);
            if (operation == null)
                return;

            while (operation.isDone == false)
                await Task.Yield();
        }

        public async Task UnloadScenesAsync(List<string> scenesToUnload) {
            foreach (string sceneToUnload in scenesToUnload.ToList())
                await UnloadSceneAsync(sceneToUnload);
        }

        private static void ActivateFallbackIfActive(string sceneToUnload) {
            Scene scene = SceneManager.GetSceneByName(sceneToUnload);
            if (scene.IsValid() == false || SceneManager.GetActiveScene() != scene)
                return;

            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene candidate = SceneManager.GetSceneAt(i);
                if (candidate.IsValid() == false || candidate.isLoaded == false)
                    continue;
                if (candidate.name == sceneToUnload || candidate.name == "DontDestroyOnLoad")
                    continue;

                SceneManager.SetActiveScene(candidate);
                return;
            }
        }
    }
}
