using System;
using System.Threading.Tasks;
using Features.GameFlowStateMachineModule.Scripts;
using Features.GameFlowStateMachineModule.Scripts.States;
using Features.MvpModule;
using Game.Connection;
using UnityEngine;

namespace Features.MenuModule.Scripts {
    public sealed class MenuSessionPresenter : PresenterBehaviour<MenuSessionViewBase> {
        private readonly IConnectionSessionService _connectionSession;
        private readonly ISteamLobbyService _steamLobby;
        private readonly IGameFlowStateMachineService _gameFlowStateMachine;
        private bool _isBusy;

        public MenuSessionPresenter(
            IConnectionSessionService connectionSession,
            ISteamLobbyService steamLobby,
            IGameFlowStateMachineService gameFlowStateMachine) {
            _connectionSession = connectionSession;
            _steamLobby = steamLobby;
            _gameFlowStateMachine = gameFlowStateMachine;
        }

        protected override void OnViewSet() {
            View.OnHostClicked += OnHostClicked;
            View.OnJoinClicked += OnJoinClicked;
            View.OnHostSteamClicked += OnHostSteamClicked;
            View.OnJoinSteamClicked += OnJoinSteamClicked;
            _steamLobby.JoinRequested += OnSteamJoinRequested;
            View.SetStatus(string.Empty);
        }

        protected override void OnDisposed() {
            View.OnHostClicked -= OnHostClicked;
            View.OnJoinClicked -= OnJoinClicked;
            View.OnHostSteamClicked -= OnHostSteamClicked;
            View.OnJoinSteamClicked -= OnJoinSteamClicked;
            _steamLobby.JoinRequested -= OnSteamJoinRequested;
        }

        void OnHostClicked() =>
            _ = RunAsync(HostAsync);

        void OnJoinClicked() =>
            _ = RunAsync(JoinAsync);

        void OnHostSteamClicked() =>
            _ = RunAsync(HostSteamAsync);

        void OnJoinSteamClicked() =>
            _ = RunAsync(JoinSteamAsync);

        void OnSteamJoinRequested(ulong lobbyId) =>
            _ = RunAsync(() => JoinSteamLobbyAsync(lobbyId));

        async Task HostAsync() {
            await _connectionSession.HostAsync();
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
        }

        async Task JoinAsync() {
            await _connectionSession.JoinAsync(View.Address);
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
        }

        async Task HostSteamAsync() {
            await _connectionSession.HostSteamAsync();
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
        }

        async Task JoinSteamAsync() {
            if (ulong.TryParse(View.Address.Trim(), out ulong lobbyId) == false)
                throw new InvalidOperationException("Enter a Steam lobby id.");

            await JoinSteamLobbyAsync(lobbyId);
        }

        async Task JoinSteamLobbyAsync(ulong lobbyId) {
            await _connectionSession.JoinSteamAsync(lobbyId);
            await _gameFlowStateMachine.EnterAsync<SessionGameFlowState>();
        }

        async Task RunAsync(Func<Task> operation) {
            if (_isBusy)
                return;

            _isBusy = true;
            SetInteractable(false);
            SetStatus("Connecting...");

            try {
                await operation();
                if (_steamLobby.CurrentLobbyId.HasValue)
                    SetStatus($"Steam lobby {_steamLobby.CurrentLobbyId.Value}");
                else
                    SetStatus("Connected.");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
                SetStatus(exception.Message);
            }
            finally {
                _isBusy = false;
                SetInteractable(true);
            }
        }

        void SetStatus(string message) {
            if (View != null && View.IsViewDisposed == false)
                View.SetStatus(message);
        }

        void SetInteractable(bool interactable) {
            if (View != null && View.IsViewDisposed == false)
                View.SetInteractable(interactable);
        }
    }
}
