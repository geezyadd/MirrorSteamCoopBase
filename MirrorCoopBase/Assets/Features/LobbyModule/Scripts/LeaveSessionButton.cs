using System;
using System.Threading.Tasks;
using Features.GameFlowStateMachineModule.Scripts;
using Features.GameFlowStateMachineModule.Scripts.States;
using Game.Connection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Features.LobbyModule.Scripts {
    [RequireComponent(typeof(Button))]
    public sealed class LeaveSessionButton : MonoBehaviour {
        private IConnectionSessionService _connectionSession;
        private IGameFlowStateMachineService _gameFlowStateMachine;
        private Button _button;
        private bool _isBusy;

        [Inject]
        private void Construct(
            IConnectionSessionService connectionSession,
            IGameFlowStateMachineService gameFlowStateMachine) {
            _connectionSession = connectionSession;
            _gameFlowStateMachine = gameFlowStateMachine;
        }

        private void Awake() {
            _button = GetComponent<Button>();
        }

        private void OnEnable() {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable() {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked() {
            _ = LeaveAsync();
        }

        private async Task LeaveAsync() {
            if (_isBusy)
                return;

            _isBusy = true;
            if (_button != null)
                _button.interactable = false;

            try {
                await _connectionSession.StopToMenuAsync();
                await _gameFlowStateMachine.EnterAsync<MenuGameFlowState>();
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
            finally {
                _isBusy = false;
                if (_button != null)
                    _button.interactable = true;
            }
        }
    }
}
