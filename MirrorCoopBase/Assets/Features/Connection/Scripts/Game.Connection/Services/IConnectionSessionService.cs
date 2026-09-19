using System.Threading.Tasks;

namespace Game.Connection
{
    public interface IConnectionSessionService
    {
        Task HostAsync();
        Task JoinAsync(string address);
        Task HostSteamAsync();
        Task JoinSteamAsync(ulong lobbyId);
        Task StopToMenuAsync();
    }
}
