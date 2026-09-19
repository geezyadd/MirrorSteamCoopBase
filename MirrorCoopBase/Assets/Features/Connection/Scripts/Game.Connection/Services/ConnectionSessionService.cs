using System;
using System.Threading.Tasks;

namespace Game.Connection
{
    public sealed class ConnectionSessionService : IConnectionSessionService
    {
        private readonly ConnectionSessionModel _model;

        public ConnectionSessionService(ConnectionSessionModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public async Task HostAsync()
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            await networkManager.PrepareLobbySceneAsync();
            await networkManager.UnloadSceneIfLoadedAsync(networkManager.MenuSceneName);
            networkManager.StartHost();
        }

        public async Task JoinAsync(string address)
        {
            ConnectionNetworkManager networkManager = RequireNetworkManager();
            networkManager.networkAddress = string.IsNullOrWhiteSpace(address) ? "localhost" : address;
            await networkManager.PrepareLobbySceneAsync();
            await networkManager.UnloadSceneIfLoadedAsync(networkManager.MenuSceneName);
            networkManager.StartClient();
        }

        public Task StopToMenuAsync()
        {
            RequireNetworkManager().StopSession();
            return Task.CompletedTask;
        }

        ConnectionNetworkManager RequireNetworkManager()
        {
            if (_model.NetworkManager != null)
                return _model.NetworkManager;

            throw new InvalidOperationException("ConnectionSessionService: NetworkManager is not registered in ConnectionSessionModel.");
        }
    }
}
