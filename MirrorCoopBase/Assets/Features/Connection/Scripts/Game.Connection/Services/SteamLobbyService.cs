using System;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using Zenject;

namespace Game.Connection
{
    public sealed class SteamLobbyService : ISteamLobbyService, IInitializable, ITickable, IDisposable
    {
        private readonly SteamLobbyModel _model;
        private CallResult<LobbyCreated_t> _lobbyCreated;
        private CallResult<LobbyEnter_t> _lobbyEnter;
        private Callback<GameLobbyJoinRequested_t> _joinRequested;
        private TaskCompletionSource<bool> _createTcs;
        private TaskCompletionSource<bool> _joinTcs;
        private bool _callbacksReady;
        private bool _steamApiMissing;

        public SteamLobbyService(SteamLobbyModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public event Action<ulong> JoinRequested;

        public bool IsAvailable
        {
            get
            {
                if (_steamApiMissing)
                    return false;

                try
                {
                    return SteamAPI.IsSteamRunning() && SteamUser.GetSteamID().IsValid();
                }
                catch (DllNotFoundException)
                {
                    _steamApiMissing = true;
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public ulong? CurrentLobbyId =>
            _model.HasLobby ? (ulong)_model.CurrentLobby.Value : null;

        public ulong HostSteamId
        {
            get
            {
                if (_model.HasLobby == false)
                    throw new InvalidOperationException("Steam lobby is not created.");

                return SteamMatchmaking.GetLobbyOwner(_model.CurrentLobby.Value).m_SteamID;
            }
        }

        public void Initialize()
        {
            ConnectionNetworkManager.MapLoadStarted += OnMapLoadStarted;
            ConnectionNetworkManager.ServerStopped += OnNetworkStopped;
            ConnectionNetworkManager.ClientStopped += OnNetworkStopped;
            EnsureCallbacks();
        }

        public void Tick()
        {
            if (_callbacksReady)
                return;

            EnsureCallbacks();
        }

        public void Dispose()
        {
            ConnectionNetworkManager.MapLoadStarted -= OnMapLoadStarted;
            ConnectionNetworkManager.ServerStopped -= OnNetworkStopped;
            ConnectionNetworkManager.ClientStopped -= OnNetworkStopped;
            _lobbyCreated?.Dispose();
            _lobbyEnter?.Dispose();
            _joinRequested?.Dispose();
        }

        public async Task CreateFriendsLobbyAsync(int maxPlayers)
        {
            EnsureSteam();
            EnsureCallbacks();
            LeaveLobby();

            _createTcs = new TaskCompletionSource<bool>();
            _lobbyCreated.Set(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, Mathf.Max(1, maxPlayers)));
            await Await(_createTcs.Task, "Steam lobby create timed out.");
        }

        public async Task JoinLobbyAsync(ulong lobbyId)
        {
            EnsureSteam();
            EnsureCallbacks();
            LeaveLobby();

            CSteamID lobby = new CSteamID(lobbyId);
            if (lobby.IsValid() == false)
                throw new InvalidOperationException("Steam lobby id is invalid.");

            _joinTcs = new TaskCompletionSource<bool>();
            _lobbyEnter.Set(SteamMatchmaking.JoinLobby(lobby));
            await Await(_joinTcs.Task, "Steam lobby join timed out.");
        }

        public void LeaveLobby()
        {
            _createTcs?.TrySetCanceled();
            _joinTcs?.TrySetCanceled();
            _createTcs = null;
            _joinTcs = null;

            if (_model.HasLobby)
            {
                try
                {
                    SteamMatchmaking.LeaveLobby(_model.CurrentLobby.Value);
                }
                catch
                {
                    // Steam may already be shut down.
                }
            }

            _model.CurrentLobby = null;
        }

        public void SetLobbyJoinable(bool joinable)
        {
            if (_model.HasLobby == false)
                return;

            SteamMatchmaking.SetLobbyJoinable(_model.CurrentLobby.Value, joinable);
        }

        void EnsureCallbacks()
        {
            if (_callbacksReady || IsAvailable == false)
                return;

            _lobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
            _lobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
            _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            _callbacksReady = true;
        }

        void OnLobbyCreated(LobbyCreated_t callback, bool ioFailure)
        {
            if (ioFailure || callback.m_eResult != EResult.k_EResultOK)
            {
                _createTcs?.TrySetException(new InvalidOperationException($"Failed to create Steam lobby: {callback.m_eResult}"));
                return;
            }

            CSteamID lobbyId = (CSteamID)callback.m_ulSteamIDLobby;
            _model.CurrentLobby = lobbyId;
            string lobbyName = SteamFriends.GetPersonaName() + "'s room";
            SteamMatchmaking.SetLobbyData(lobbyId, "name", lobbyName);
            Debug.Log($"Steam lobby created: {lobbyId.m_SteamID}");
            _createTcs?.TrySetResult(true);
        }

        void OnLobbyEntered(LobbyEnter_t callback, bool ioFailure)
        {
            if (ioFailure || callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                _joinTcs?.TrySetException(new InvalidOperationException("Failed to join Steam lobby."));
                return;
            }

            _model.CurrentLobby = (CSteamID)callback.m_ulSteamIDLobby;
            _joinTcs?.TrySetResult(true);
        }

        void OnJoinRequested(GameLobbyJoinRequested_t callback)
        {
            JoinRequested?.Invoke(callback.m_steamIDLobby.m_SteamID);
        }

        void OnMapLoadStarted()
        {
            SetLobbyJoinable(false);
        }

        void OnNetworkStopped()
        {
            LeaveLobby();
        }

        void EnsureSteam()
        {
            if (IsAvailable)
                return;

            throw new InvalidOperationException("Steam is not running or Steamworks failed to initialize.");
        }

        static async Task Await(Task task, string timeoutMessage)
        {
            Task finished = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(20)));
            if (finished != task)
                throw new TimeoutException(timeoutMessage);

            await task;
        }
    }
}
