using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicOnionServer
{
    public class GameHub : StreamingHubBase<IGameHub, IGameHubReceiver>, IGameHub
    {
        IGroup room;
        Player self;
        IInMemoryStorage<Player> playerStorage;
        static int[] board = new int[9];
        int players;

        public async Task<Player[]> JoinAsync()
        {
            const string roomName = "SampleRoom";
            room = await Group.AddAsync(roomName);
            int _idPlayer = (await room.GetMemberCountAsync() > 1) ? 2 : 1;
            await room.RemoveAsync(Context);
            self = new Player() { IdPlayer = _idPlayer, IdFill = _idPlayer};
            Console.Write("\nPlayer " + self.IdPlayer);
            (room, playerStorage) = await Group.AddAsync(roomName, self);
            Broadcast(room).OnJoin(self);
            return playerStorage.AllValues.ToArray();
        }

        public async Task LeaveAsync()
        {
            await room.RemoveAsync(Context);
            Broadcast(room).OnLeave(self);
        }

        public async Task MoveAsync(Player _player)
        {
            board[_player.ButtonId] = _player.IdFill;
            self.ButtonId = _player.ButtonId;
            Console.Write("\nInput Self " + self.IdPlayer + ": board = " + self.ButtonId + " fill = " + self.IdFill);
            Console.Write("\nBoard " + _player.ButtonId + ": fill = " + board[_player.ButtonId]);
            //for(int i = 0; i < board.Length; i++)
            //{
            //    Console.Write("Board " + i + ": " + board[i]);
            //    Console.Write("|");
            //}
            Broadcast(room).OnMove(self);
        }

        public async Task CheckStateGameAsync()
        {
            if (CheckBoard(self.IdPlayer, self.ButtonId))
                Broadcast(room).OnCheckState(true, self.IdPlayer);
            else
                Broadcast(room).OnCheckState(false, self.IdPlayer);
        }
        
        private bool CheckBoard(int _idPlayer, int _IdButton)
        {
            if (CheckHorizontal(_IdButton, _idPlayer))
            {
                Console.WriteLine("CheckHorizontal true");
                return true;
            }
            if(CheckVertical(_IdButton, _idPlayer))
            {
                Console.WriteLine("CheckVertical true");
                return true;
            }
            if (CheckDiagonalLeft(_idPlayer))
            {
                Console.WriteLine("DiagonalLeft true");
                return true;
            }
            if (CheckDiagonalRight(_idPlayer))
            {
                Console.WriteLine("DiagonalRight true");
                return true;
            }
            Console.WriteLine("CheckBoard false");
            return false;
        }

        private bool CheckHorizontal(int _IdButton, int _idFillPlayer)
        {
            int row = (int)(MathF.Ceiling((_IdButton + 1) / 3));
            Console.WriteLine("\nCH Row: " + row);
            if ((board[((row - 1) * 3 + 1) -1]  == _idFillPlayer) &&
                (board[((row - 1) * 3 + 2) -1]  == _idFillPlayer) &&
                (board[((row - 1) * 3 + 3) -1]  == _idFillPlayer))
            {
                return true;
            }
            return false;
        }

        private bool CheckVertical(int _IdButton, int _idFillPlayer)
        {
            int col = (int)MathF.Ceiling((_IdButton + 1) % 3);
            Console.WriteLine("\nCH col: " + col);
            if ((board[col + 1 - 2]) == _idFillPlayer &&
                (board[col + 4 - 2]) == _idFillPlayer &&
                (board[col + 7 - 2]) == _idFillPlayer)
            {
                return true;
            }
            return false;
        }

        private bool CheckDiagonalLeft(int _idFillPlayer)
        {
            if (board[0] == _idFillPlayer &&
                board[4] == _idFillPlayer &&
                board[8] == _idFillPlayer)
            {
                return true;
            }
            return false;
        }

        private bool CheckDiagonalRight(int _idFillPlayer)
        {
            if (board[2] == _idFillPlayer &&
                board[4] == _idFillPlayer &&
                board[6] == _idFillPlayer)
            {
                return true;
            }
            return false;
        }


        protected override ValueTask OnDisconnected()
        {
            return CompletedTask;
        }
    }
}
