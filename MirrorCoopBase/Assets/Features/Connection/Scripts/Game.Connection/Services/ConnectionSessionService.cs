using System;
using System.Threading.Tasks;

namespace Game.Connection
{
    public sealed class ConnectionSessionService : IConnectionSessionService
    {
        private readonly ConnectionSessionModel _model;
        private readonly ISteamLobbyService _steamLobby;

        public ConnectionSessionService(ConnectionSessionModel model, ISteamLobbyService steamLobby)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _steamLobby = steamLobby ?? throw new ArgumentNullException(nameof(steamLobby));
        }

        public async Task HostAsync()
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            PrepareDirect(networkManager);
            await networkManager.PrepareLobbySceneAsync();
            await networkManager.UnloadSceneIfLoadedAsync(networkManager.MenuSceneName);
            networkManager.StartHost();
        }

        public async Task JoinAsync(string address)
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            PrepareDirect(networkManager);
            networkManager.networkAddress = string.IsNullOrWhiteSpace(address) ? "localhost" : address;
            await networkManager.PrepareLobbySceneAsync();
            await networkManager.UnloadSceneIfLoadedAsync(networkManager.MenuSceneName);
            networkManager.StartClient();
        }

        public async Task HostSteamAsync()
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            try
            {
                await _steamLobby.CreateFriendsLobbyAsync(networkManager.maxConnections);
                networkManager.SetUseSteamTransport(true);
                networkManager.networkAddress = _steamLobby.HostSteamId.ToString();
                await networkManager.PrepareLobbySceneAsync();
                await networkManager.UnloadSceneIfLoadedAsync(networkManager.MenuSceneName);
                networkManager.StartHost();
            }
            catch
            {
                ResetSteam(networkManager);
                throw;
            }
        }

        public async Task JoinSteamAsync(ulong lobbyId)
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            try
            {
                await _steamLobby.JoinLobbyAsync(lobbyId);
                networkManager.SetUseSteamTransport(true);
                networkManager.networkAddress = _steamLobby.HostSteamId.ToString();
                await networkManager.PrepareLobbySceneAsync();
                await networkManager.UnloadSceneIfLoadedAsync(networkManager.MenuSceneName);
                networkManager.StartClient();
            }
            catch
            {
                ResetSteam(networkManager);
                throw;
            }
        }

        public Task StopToMenuAsync()
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            networkManager.StopSession();
            ResetSteam(networkManager);
            return Task.CompletedTask;
        }

        void PrepareDirect(ConnectionNetworkManager networkManager)
        {
            ResetSteam(networkManager);
        }

        void ResetSteam(ConnectionNetworkManager networkManager)
        {
            _steamLobby.LeaveLobby();
            networkManager.SetUseSteamTransport(false);
        }

        ConnectionNetworkManager RequireNetworkManager()
        {
            if (_model.NetworkManager != null)
                return _model.NetworkManager;

            throw new InvalidOperationException("ConnectionSessionService: NetworkManager is not registered in ConnectionSessionModel.");
        }
    }
}
