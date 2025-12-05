using System;
using System.Threading.Tasks;
using BattleShipGame2.Models;
using BattleShipGame2.Logic;

namespace BattleShipGame2.Services;

public class GameService : IGameService
{
    private GameBoard _player1Board;
    private GameBoard _player2Board;
    private GameBoard _opponentBoard;
    private GameMode _currentMode;
    private bool _playerTurn;
    private bool _isPlayer1Turn = true; // Кто сейчас ходит
    
    // Свойства
    private BotManager _botManager;
    private BotDifficulty _currentDifficulty;
    
    private int _playerHits;
    private int _playerMisses;
    private int _opponentHits;
    private int _opponentMisses;
    
    // События из интерфейса
    public event Action? GameStateChanged;
    public event Action? BoardsChanged;
    public event Action? BoardRedrawRequested;
    
    public bool IsPlayer1Turn => _isPlayer1Turn;
    public bool IsTwoPlayerMode => _currentMode == GameMode.VsPlayer;
    public GameBoard PlayerBoard => _isPlayer1Turn ? _player1Board : _player2Board;
    public GameBoard Player1Board => _player1Board;
    public GameBoard Player2Board => _player2Board;
    public GameBoard OpponentBoard => _opponentBoard;
    public GameMode CurrentMode => _currentMode;
    public bool PlayerTurn => _playerTurn;
    
    public int PlayerHits => _playerHits;
    public int PlayerMisses => _playerMisses;
    public int OpponentHits => _opponentHits;
    public int OpponentMisses => _opponentMisses;
    
    public GameService()
    {
        _botManager = new BotManager();
        _player1Board = new GameBoard();
        _player2Board = new GameBoard();
        _opponentBoard = new GameBoard();
    }
    
    private void OnBoardRedrawRequested() => BoardRedrawRequested?.Invoke();

    
    public void StartNewGame(GameMode mode, BotDifficulty difficulty = BotDifficulty.Easy)
    {
        _currentMode = mode;
        _currentDifficulty = difficulty;
        _playerTurn = true;
        _isPlayer1Turn = true;
        
        _player1Board = new GameBoard();
        _player2Board = new GameBoard();
        _opponentBoard = new GameBoard();
        
        _playerHits = 0;
        _playerMisses = 0;
        _opponentHits = 0;
        _opponentMisses = 0;
        
        if (mode == GameMode.VsComputer)
        {
            _botManager.SetDifficulty(difficulty);
            _botManager.ResetAll();
        }
        
        OnGameStateChanged();
        OnBoardsChanged();
    }
    
    public async Task<(bool hit, bool sunk, bool gameOver)> PlayerAttack(int x, int y)
    {
        if (!_playerTurn) 
            return (false, false, false);
        
        GameBoard targetBoard;
        
        if (_currentMode == GameMode.VsPlayer)
        {
            // В режиме 1 на 1 атакуем доску другого игрока
            targetBoard = _isPlayer1Turn ? _player2Board : _player1Board;
        }
        else
        {
            // В режиме против компьютера
            targetBoard = _opponentBoard;
        }
        
        if (!targetBoard.CanAttackCell(x, y))
            return (false, false, false);
        
        var result = _opponentBoard.Attack(x, y);
        
        if (result.hit)
        {
            if (_currentMode == GameMode.VsPlayer)
            {
                // В режиме 1 на 1 считаем попадания отдельно
                if (_isPlayer1Turn) _playerHits++;
                else _opponentHits++;
            }
            else
            {
                _playerHits++;
            }
            
            SoundManager.PlayHit();
            
            if (result.sunk)
            {
                SoundManager.PlaySunk();
                if (result.gameOver)
                {
                    SoundManager.PlayWin();
                    _playerTurn = false;
                }
            }
        }
        else if (targetBoard.Grid[x, y] == CellState.Miss)
        {
            if (_currentMode == GameMode.VsPlayer)
            {
                if (_isPlayer1Turn) _playerMisses++;
                else _opponentMisses++;
            }
            else
            {
                _playerMisses++;
            }
            
            SoundManager.PlayMiss();
            _playerTurn = false;
            
            // Меняем ход в режиме 1 на 1
            if (_currentMode == GameMode.VsPlayer)
            {
                _isPlayer1Turn = !_isPlayer1Turn;
                _playerTurn = true; // Передаем ход другому игроку
            }
            else if (_currentMode == GameMode.VsComputer && !result.gameOver)
            {
                await Task.Delay(800);
                await ComputerTurn();
            }
        }
        
        OnGameStateChanged();
        OnBoardsChanged();
        OnBoardRedrawRequested();
        
        return result;
    }
    
    public void SwitchToPlayer2Placement()
    {
        _isPlayer1Turn = false;
        OnGameStateChanged();
        OnBoardsChanged();
    }
    
    public async Task ComputerTurn()
    {
        bool continueTurn = true;

        while (continueTurn && !_playerTurn)
        {
            var result = await (_currentDifficulty == BotDifficulty.Easy 
                ? _botManager.MakeSimpleTurn(_player1Board, HandleBotAttack)
                : _botManager.MakeSmartTurn(_player1Board, HandleBotAttack));
            
            continueTurn = result.ContinueTurn && !result.GameOver;
            
            if (continueTurn)
            {
                await Task.Delay(500);
            }
            
            if (!continueTurn && !result.GameOver)
            {
                _playerTurn = true;
            }
        }
        
        OnGameStateChanged();
        OnBoardsChanged();
    }
    
    private void HandleBotAttack(int x, int y, bool hit, bool sunk, bool gameOver)
    {
        if (hit)
        {
            _opponentHits++;
            SoundManager.PlayHit();
            
            if (sunk)
            {
                SoundManager.PlaySunk();
                if (gameOver)
                {
                    SoundManager.PlayLose();
                    _playerTurn = false;
                }
            }
        }
        else
        {
            _opponentMisses++;
            SoundManager.PlayMiss();
        }
        
        OnGameStateChanged();
        OnBoardsChanged();
    }
    
    public void PlaceShipsRandomly(bool isPlayerBoard)
    {
        var board = isPlayerBoard ? _player1Board : _opponentBoard;
        board.PlaceShipsRandomly();
        OnBoardsChanged();
    }
    
    public bool CanPlaceShip(int x, int y, int size, bool horizontal)
    {
        return _player1Board.CanPlaceShip(x, y, size, horizontal);
    }
    
    // В методе PlaceShip GameService:
    public bool PlaceShip(int x, int y, int size, bool horizontal)
    {
        var ship = new Ship(size, horizontal);
        var result = _player1Board.PlaceShip(ship, x, y);
        if (result)
        {
            OnBoardsChanged();
        }
        return result;
    }
    
    
    public void ResetGame()
    {
        _player1Board = new GameBoard();
        _opponentBoard = new GameBoard();
        _playerHits = 0;
        _playerMisses = 0;
        _opponentHits = 0;
        _opponentMisses = 0;
        _playerTurn = true;
        
        OnGameStateChanged();
        OnBoardsChanged();
    }
    
    // Методы для вызова событий
    private void OnGameStateChanged() => GameStateChanged?.Invoke();
    private void OnBoardsChanged() => BoardsChanged?.Invoke();
}