using System;
using System.Threading.Tasks;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BattleShipGame2.Services;
using BattleShipGame2.Models;

namespace BattleShipGame2.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IGameService _gameService;
    
    [ObservableProperty]
    private string _statusText = "Ваш ход!";
    
    [ObservableProperty]
    private string _playerStats = "Ваши выстрелы: 0 попаданий, 0 промахов";
    
    [ObservableProperty]
    private string _opponentStats = "Выстрелы противника: 0 попаданий, 0 промахов";
    
    [ObservableProperty]
    private bool _canAttack = true;
    
    [ObservableProperty]
    private GameBoard? _playerBoard;
    
    [ObservableProperty]
    private GameBoard? _opponentBoard;
    
    public GameViewModel(INavigationService navigationService, IGameService gameService)
    {
        _navigationService = navigationService;
        _gameService = gameService;
    
        // Подписываемся на события GameService
        _gameService.GameStateChanged += OnGameStateChanged;
        _gameService.BoardsChanged += OnBoardsChanged;
        _gameService.BoardRedrawRequested += OnBoardRedrawRequested; // НОВОЕ
    
        UpdateAll();
    }

    private void OnBoardRedrawRequested()
    {
        Console.WriteLine("BoardRedrawRequested received!");
    
        // Принудительно вызываем перерисовку
        OnPropertyChanged(nameof(PlayerBoard));
        OnPropertyChanged(nameof(OpponentBoard));
    
        // Можно также добавить специальное свойство для принудительной перерисовки
        RedrawBoards();
    }

    private void RedrawBoards()
    {
        // Этот метод будет вызываться из View через binding
        // или мы можем использовать его для прямого вызова RenderBoards
        Console.WriteLine("RedrawBoards called");
    
        // Здесь мы обновляем свойства, которые триггерят перерисовку
        var tempPlayerBoard = PlayerBoard;
        var tempOpponentBoard = OpponentBoard;
    
        PlayerBoard = null;
        OpponentBoard = null;
    
        // Даем Avalonia время обработать изменения
        Task.Delay(10).ContinueWith(_ =>
        {
            // Возвращаем значения обратно в UI потоке
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                PlayerBoard = tempPlayerBoard;
                OpponentBoard = tempOpponentBoard;
            });
        });
    }
    
    private void OnGameStateChanged()
    {
        UpdateStats();
        UpdateStatus();
    }
    
    private void OnBoardsChanged()
    {
        // Принудительно уведомляем об изменении свойств
        OnPropertyChanged(nameof(PlayerBoard));
        OnPropertyChanged(nameof(OpponentBoard));
    
        // Также обновляем ссылки (хотя это те же объекты)
        PlayerBoard = _gameService.PlayerBoard;
        OpponentBoard = _gameService.OpponentBoard;
    }
    
    [RelayCommand]
    private async Task Attack((int x, int y) coords)
    {
        Console.WriteLine($"AttackCommand executed: ({coords.x},{coords.y})");
    
        if (!CanAttack || OpponentBoard == null)
        {
            Console.WriteLine($"Cannot attack: CanAttack={CanAttack}, OpponentBoard={OpponentBoard}");
            return;
        }
    
        // Проверяем, можно ли атаковать эту клетку
        var cellState = OpponentBoard.Grid[coords.x, coords.y];
        Console.WriteLine($"Cell state at ({coords.x},{coords.y}): {cellState}");
    
        if (cellState != CellState.Empty && cellState != CellState.Ship)
        {
            Console.WriteLine("Cell already attacked!");
            return;
        }
    
        CanAttack = false;
        Console.WriteLine("Calling PlayerAttack...");
    
        var (hit, sunk, gameOver) = await _gameService.PlayerAttack(coords.x, coords.y);
        Console.WriteLine($"Attack result: hit={hit}, sunk={sunk}, gameOver={gameOver}");
    
        if (gameOver)
        {
            StatusText = "🎉 ПОБЕДА! Вы потопили весь флот!";
            CanAttack = false;
            return;
        }
    
        if (hit)
        {
            StatusText = sunk ? "💥 Корабль потоплен!" : "🔥 ПОПАДАНИЕ!";
            CanAttack = true;
        }
        else
        {
            StatusText = "💧 Промах! Ход противника...";
            await Task.Delay(500);
        
            // Обновляем статус после хода компьютера
            UpdateStatus();
            CanAttack = _gameService.PlayerTurn;
        }
    
        Console.WriteLine($"After attack: CanAttack={CanAttack}");
    }
    
    [RelayCommand]
    private void NewGame()
    {
        _gameService.ResetGame();
        _navigationService.NavigateTo<MenuViewModel>();
    }
    
    [RelayCommand]
    private void BackToMenu()
    {
        _navigationService.NavigateTo<MenuViewModel>();
    }
    
    private void UpdateAll()
    {
        PlayerBoard = _gameService.PlayerBoard;
        OpponentBoard = _gameService.OpponentBoard;
        UpdateStats();
        UpdateStatus();
    }
    
    private void UpdateStats()
    {
        PlayerStats = $"🎯 Ваши выстрелы: {_gameService.PlayerHits} попаданий, {_gameService.PlayerMisses} промахов";
        OpponentStats = $"💣 Выстрелы противника: {_gameService.OpponentHits} попаданий, {_gameService.OpponentMisses} промахов";
    }
    
    private void UpdateStatus()
    {
        StatusText = _gameService.PlayerTurn 
            ? "⚔️ ВАШ ХОД! Атакуйте поле противника" 
            : "💀 Ход противника...";
        CanAttack = _gameService.PlayerTurn;
    }
    
    // Отписка от событий
    public void Dispose()
    {
        _gameService.GameStateChanged -= OnGameStateChanged;
        _gameService.BoardsChanged -= OnBoardsChanged;
    }
}