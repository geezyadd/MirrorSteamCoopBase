using System;
using System.Threading.Tasks;
using Features.GameFlowStateMachineModule.Scripts;
using Features.GameFlowStateMachineModule.Scripts.States;
using Features.MvpModule;
using Game.Connection;
using UnityEngine;

namespace Features.LobbyModule.Scripts {
    public sealed class LeaveSessionPresenter : PresenterBehaviour<LeaveSessionViewBase> {
        private readonly IConnectionSessionService _connectionSession;
        private readonly IGameFlowStateMachineService _gameFlowStateMachine;
        private bool _isBusy;

        public LeaveSessionPresenter(
            IConnectionSessionService connectionSession,
            IGameFlowStateMachineService gameFlowStateMachine) {
            _connectionSession = connectionSession;
            _gameFlowStateMachine = gameFlowStateMachine;
        }

        protected override void OnViewSet() {
            View.OnLeaveClicked += OnLeaveClicked;
        }

        protected override void OnDisposed() {
            View.OnLeaveClicked -= OnLeaveClicked;
        }

        void OnLeaveClicked() =>
            _ = LeaveAsync();

        async Task LeaveAsync() {
            if (_isBusy)
                return;

            _isBusy = true;
            if (View != null && View.IsViewDisposed == false)
                View.SetInteractable(false);

            try {
                await _connectionSession.StopToMenuAsync();
                await _gameFlowStateMachine.EnterAsync<MenuGameFlowState>();
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
            finally {
                _isBusy = false;
                if (View != null && View.IsViewDisposed == false)
                    View.SetInteractable(true);
            }
        }
    }
}
