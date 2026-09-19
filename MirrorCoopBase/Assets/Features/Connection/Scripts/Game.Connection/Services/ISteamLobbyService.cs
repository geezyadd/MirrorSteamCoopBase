using System;
using System.Threading.Tasks;

namespace Game.Connection
{
    public interface ISteamLobbyService
    {
        bool IsAvailable { get; }
        ulong? CurrentLobbyId { get; }
        ulong HostSteamId { get; }

        event Action<ulong> JoinRequested;

        Task CreateFriendsLobbyAsync(int maxPlayers);
        Task JoinLobbyAsync(ulong lobbyId);
        void LeaveLobby();
        void SetLobbyJoinable(bool joinable);
    }
}
