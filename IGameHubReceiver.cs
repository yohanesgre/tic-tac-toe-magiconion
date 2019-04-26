using System.Threading.Tasks;

public interface IGameHubReceiver
{
    void OnJoin(Player _player);
    void OnLeave(Player _player);
    void OnMove(Player _player);
    void OnCheckState(bool _isGameOver, int _winner);
}
