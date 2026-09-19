using System;
using System.Threading.Tasks;
using Features.GameCoreModule.Scripts.Constants;
using Features.SceneLoaderModule.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Features.BootstrapersModule.Scripts {
    public sealed class BootstrapSceneBootstrapper : MonoBehaviour {
        private ISceneLoaderService _sceneLoaderService;
        private bool _started;

        [Inject]
        private void Construct(ISceneLoaderService sceneLoaderService) {
            _sceneLoaderService = sceneLoaderService;
        }

        private void Start() {
            _ = BootstrapAsync();
        }

        private async Task BootstrapAsync() {
            if (_started)
                return;

            _started = true;

            try {
                Scene bootstrapScene = gameObject.scene;
                await _sceneLoaderService.LoadSceneAsync(SceneNames.Global, false);

                if (bootstrapScene.IsValid() && bootstrapScene.isLoaded) {
                    AsyncOperation unload = SceneManager.UnloadSceneAsync(bootstrapScene);
                    if (unload != null) {
                        while (unload.isDone == false)
                            await Task.Yield();
                    }
                }
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }
    }
}
