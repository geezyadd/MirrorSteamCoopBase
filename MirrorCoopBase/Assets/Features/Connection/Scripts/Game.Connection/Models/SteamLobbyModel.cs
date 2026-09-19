using Steamworks;

namespace Game.Connection
{
    public sealed class SteamLobbyModel
    {
        public CSteamID? CurrentLobby { get; internal set; }

        public bool HasLobby => CurrentLobby.HasValue && CurrentLobby.Value.IsValid();
    }
}
