using System.Threading.Tasks;
using MagicOnion;

public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
{
    Task<Player[]> JoinAsync();
    Task LeaveAsync();
    Task MoveAsync(Player player);
    Task CheckStateGameAsync();
}
