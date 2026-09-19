namespace Game.Connection
{
    public sealed class ConnectionSessionModel
    {
        public ConnectionNetworkManager NetworkManager { get; internal set; }

        public bool HasNetworkManager => NetworkManager != null;
    }
}
