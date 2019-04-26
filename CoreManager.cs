using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CoreManager : MonoBehaviour, IGameHubReceiver
{
    enum IdFill
    {
        O = 1,
        X = 2,
        E = -1
    }
    // Start is called before the first frame update
    public int IdPlayer;
    [SerializeField] private List<Button> listButtonBoard = new List<Button>();
    [SerializeField] private Text txtDebug;
    [SerializeField] private Text txtPlayer;
    [SerializeField] private Text txtTurn;
    [SerializeField] private Text txtWinner;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject panelJoin;
    [SerializeField] private InputField inputIp;
    private bool isJoin;
    private bool isTurn;
    private bool isGameOver;
    private string ipServer;

    private IGameHub _gameHub;
    void Start()
    {
        panelJoin.SetActive(true);
    }

    public void Join()
    {
        ipServer = inputIp.text;
        if (ipServer != null)
        {
            panelJoin.SetActive(false);
            Init();
        }
    }

    void Init()
    {
        IdPlayer = -1;
        isJoin = false;
        isGameOver = false;
        panel.SetActive(true);
        var channel = new Channel(ipServer+":12345", ChannelCredentials.Insecure);
        _gameHub = StreamingHubClient.Connect<IGameHub, IGameHubReceiver>(channel, this);
        JoinOrLeave();
        InitBoard();
    }

    void OnDestoy()
    {
        JoinOrLeave();
    }

    private void InitBoard()
    {
        for (int i = 0; i < listButtonBoard.Count; i++)
        {
            listButtonBoard[i].GetComponentInChildren<Text>().text = "";
            AddButtonsListener(i);
        }
    }

    private void AddButtonsListener(int i)
    {
        listButtonBoard[i].onClick.AddListener(() => OnClickButtonBoard(i));
    }

    void OnClickButtonBoard(int _IdButton)
    {
        if (IsButtonEmpty(_IdButton) && isTurn)
        {
            //FillButton(IdPlayer, _IdButton);
            MoveAsync(_IdButton);
        }
    }

    private void FillButton(int _idPlayer, int _IdButton)
    {
        if(_idPlayer == (int)IdFill.O)
        {
            listButtonBoard[_IdButton].GetComponentInChildren<Text>().text = "O";
        }
        else
        {
            listButtonBoard[_IdButton].GetComponentInChildren<Text>().text = "X";
        }
        
    }

    private int GetButtonValue(int _idButton)
    {
        string value = listButtonBoard[_idButton].GetComponentInChildren<Text>().text;
        if(value == "X")
        {
            return (int)IdFill.X;
        }
        else if(value == "O")
        {
            return (int)IdFill.O;
        }
        else
        {
            return (int)IdFill.E;
        }
    }

    private bool IsButtonEmpty(int _IdButton)
    {
        string value = listButtonBoard[_IdButton].GetComponentInChildren<Text>().text;
        if(value == "")
        {
            return true;
        }
        return false;
    }

    #region Client -> Server

    public async void JoinOrLeave()
    {
        if (isJoin)
        {
            await _gameHub.LeaveAsync();
            isJoin = false;
        }
        else
        {
            await _gameHub.JoinAsync();
            this.isJoin = true;
        }
    }

    public async void MoveAsync(int _IdButton)
    {
        if (isGameOver)
        {
            return;
        }

        var packet = new Player()
        {
            IdPlayer = IdPlayer,
            IdFill = IdPlayer,
            ButtonId = _IdButton
        };
        if (isTurn && !isGameOver)
        { 
            await _gameHub.MoveAsync(packet);
            await _gameHub.CheckStateGameAsync();
            isTurn = false;
        }
    }

    #endregion  
    #region Client <- Server
    public void OnJoin(Player _player)
    {
        if (IdPlayer == -1)
        {
            IdPlayer = _player.IdPlayer;
            txtPlayer.text += IdPlayer;
        }
        txtDebug.text += "\nIdPlayer: " + IdPlayer.ToString();
        if (_player.IdPlayer != 2)
        {
            if (IdPlayer == 1)
            {
                panel.SetActive(true);
                txtTurn.text = "Waiting for Player 2";
                txtTurn.gameObject.SetActive(true);
            }
        }
        else
        {
            if (IdPlayer == 1)
            {
                isTurn = true;
                panel.SetActive(false);
                txtTurn.gameObject.SetActive(false);
            }
            if (IdPlayer == 2)
            {
                isTurn = false;
                panel.SetActive(true);
                txtTurn.text = "Player 1 Turn";
                txtTurn.gameObject.SetActive(true);
            }
        }
    }

    public void OnLeave(Player _player)
    {
        throw new NotImplementedException();
    }

    public void OnMove(Player _player)
    {
        txtDebug.text += "\nOnMove IdPlayer = " + _player.IdPlayer;
        FillButton(_player.IdPlayer, _player.ButtonId);
        if (_player.IdPlayer != IdPlayer)
        {
            isTurn = true;
            txtTurn.gameObject.SetActive(false);
            panel.SetActive(false);
        }
        else
        {
            txtTurn.text = "Player " + _player.IdPlayer + " Turn";
            txtTurn.gameObject.SetActive(true);
            panel.SetActive(true);
        }
    }

    public void OnCheckState(bool _isGameOver, int _winner)
    {
        txtDebug.text += "\nIsGameOver: " + _isGameOver + ", Winner: " + _winner;
        isGameOver = _isGameOver;
        if (isGameOver)
        {
            panel.SetActive(true);
            txtTurn.gameObject.SetActive(false);
            txtWinner.gameObject.SetActive(true);
            txtWinner.text = "Player " + _winner + " win!";
            txtDebug.text += "\nPlayer" + _winner;
        }
    }
    #endregion
}


/* Logic

    private bool CheckBoard(int _idPlayer, int _IdButton)
        {
            int idFillPlayer = (_idPlayer == 1) ? (int)IdFill.O : (int)IdFill.X;
            if (CheckHorizontal(_IdButton, idFillPlayer) ||
               CheckVertical(_IdButton, idFillPlayer) ||
               CheckDiagonalLeft(idFillPlayer) ||
               CheckDiagonalRight(idFillPlayer))
            {
                return true;
            }
            return false;
        }

        private bool CheckHorizontal(int _IdButton, int _idFillPlayer)
        {
            int row = (int)MathF.Ceiling(_IdButton / 3) + 1;
            if (GetButtonValue(((row - 1) * 3 + 1) - 1) == _idFillPlayer &&
               GetButtonValue(((row - 1) * 3 + 2) - 1) == _idFillPlayer &&
               GetButtonValue(((row - 1) * 3 + 3) - 1) == _idFillPlayer)
            {
                return true;
            }
            return false;
        }

        private bool CheckVertical(int _IdButton, int _idFillPlayer)
        {
            int col = MathF.CeilToInt(_IdButton % 3);
            if (GetButtonValue(col - 1 + 1) == _idFillPlayer &&
                GetButtonValue(col - 1 + 4) == _idFillPlayer &&
                GetButtonValue(col - 1 + 7) == _idFillPlayer)
            {
                return true;
            }
            return false;
        }

        private bool CheckDiagonalLeft(int _idFillPlayer)
        {
            if (GetButtonValue(0) == _idFillPlayer &&
                GetButtonValue(4) == _idFillPlayer &&
                GetButtonValue(8) == _idFillPlayer)
            {
                return true;
            }
            return false;
        }

        private bool CheckDiagonalRight(int _idFillPlayer)
        {
            if (GetButtonValue(2) == _idFillPlayer &&
                GetButtonValue(4) == _idFillPlayer &&
                GetButtonValue(6) == _idFillPlayer)
            {
                return true;
            }
            return false;
        }

    */