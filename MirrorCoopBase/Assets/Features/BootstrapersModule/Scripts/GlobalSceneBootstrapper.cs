using System;
using System.Threading.Tasks;
using Features.GameFlowStateMachineModule.Scripts;
using Features.GameFlowStateMachineModule.Scripts.States;
using UnityEngine;
using Zenject;

namespace Features.BootstrapersModule.Scripts {
    public sealed class GlobalSceneBootstrapper : MonoBehaviour {
        private IGameFlowStateMachineService _gameFlowStateMachine;
        private bool _started;

        [Inject]
        private void Construct(IGameFlowStateMachineService gameFlowStateMachine) {
            _gameFlowStateMachine = gameFlowStateMachine;
        }

        private void Start() {
            _ = StartGameFlowAsync();
        }

        private async Task StartGameFlowAsync() {
            if (_started)
                return;

            _started = true;

            try {
                await _gameFlowStateMachine.EnterAsync<GlobalGameFlowState>();
                await _gameFlowStateMachine.EnterAsync<MenuGameFlowState>();
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }
    }
}
