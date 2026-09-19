using System;
using System.Threading.Tasks;
using Features.SceneLoaderModule.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Features.GameFlowStateMachineModule.Scripts {
    public sealed class GameFlowSceneTransitionSystem : IInitializable, IDisposable {
        private readonly GameFlowStateLifecycleEventClass _lifecycleEvents;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly GameFlowStateSceneMapper _sceneMapper;
        private Type _previousStateType;
        private Task _activeTransition = Task.CompletedTask;

        public GameFlowSceneTransitionSystem(
            GameFlowStateLifecycleEventClass lifecycleEvents,
            ISceneLoaderService sceneLoaderService,
            GameFlowStateSceneMapper sceneMapper) {
            _lifecycleEvents = lifecycleEvents;
            _sceneLoaderService = sceneLoaderService;
            _sceneMapper = sceneMapper;
        }

        public Task WaitForActiveTransitionAsync() => _activeTransition ?? Task.CompletedTask;

        public void Initialize() {
            _lifecycleEvents.OnStateEntered += OnStateEntered;
        }

        public void Dispose() {
            _lifecycleEvents.OnStateEntered -= OnStateEntered;
        }

        private void OnStateEntered(Type stateType) {
            Type previousStateType = _previousStateType;
            _previousStateType = stateType;
            _activeTransition = ApplyStateAsync(stateType, previousStateType);
            _ = ObserveTransition(_activeTransition);
        }

        private static async Task ObserveTransition(Task transition) {
            try {
                await transition;
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }

        private async Task ApplyStateAsync(Type currentStateType, Type previousStateType) {
            if (_sceneMapper.TryGetScene(previousStateType, out string sceneToUnload))
                await _sceneLoaderService.UnloadSceneAsync(sceneToUnload);

            if (_sceneMapper.TryGetScene(currentStateType, out string sceneToLoad) == false)
                return;

            await _sceneLoaderService.LoadSceneAsync(sceneToLoad, false);

            Scene scene = SceneManager.GetSceneByName(sceneToLoad);
            if (scene.IsValid() && scene.isLoaded)
                SceneManager.SetActiveScene(scene);
        }
    }
}
