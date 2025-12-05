using System;
using System.Threading.Tasks;
using BattleShipGame2.Models;
using BattleShipGame2.Logic;

namespace BattleShipGame2.Services;

public interface IGameService
{
    // События для уведомления об изменениях
    event Action? GameStateChanged;
    event Action? BoardsChanged;
    event Action? BoardRedrawRequested;
    
    // Свойства
    GameBoard PlayerBoard { get; }
    GameBoard OpponentBoard { get; }
    GameMode CurrentMode { get; }
    bool PlayerTurn { get; }
    
    // Текущий игрок (true - игрок 1, false - игрок 2)
    bool IsPlayer1Turn { get; }
    
    // Добавить свойство для режима "два игрока"
    bool IsTwoPlayerMode { get; }
    
    // Доска второго игрока (в режиме 1 на 1)
    GameBoard Player2Board { get; }
    
    int PlayerHits { get; }
    int PlayerMisses { get; }
    int OpponentHits { get; }
    int OpponentMisses { get; }
    
    // Методы
    void StartNewGame(GameMode mode, BotDifficulty difficulty = BotDifficulty.Easy);
    Task<(bool hit, bool sunk, bool gameOver)> PlayerAttack(int x, int y);
    Task ComputerTurn();
    void PlaceShipsRandomly(bool isPlayerBoard);
    bool CanPlaceShip(int x, int y, int size, bool horizontal);
    bool PlaceShip(int x, int y, int size, bool horizontal);
    void ResetGame();
}