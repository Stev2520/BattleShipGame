using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BattleShipGame2.Services;
using BattleShipGame2.Logic;
using BattleShipGame2.Models;

namespace BattleShipGame2.ViewModels;

public partial class ShipPlacementViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IGameService _gameService;
    private readonly GameMode _gameMode;
    private readonly BotDifficulty _botDifficulty;

    [ObservableProperty]
    private string _statusText = "Расставьте корабли";

    [ObservableProperty]
    private int _currentShipIndex = 0;

    [ObservableProperty]
    private bool _isHorizontal = true;

    [ObservableProperty]
    private GameBoard? _placementBoard;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
    private bool _allShipsPlaced = false;

    private List<int> _shipsToPlace = new() { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

    public int CurrentShipSize => _currentShipIndex < _shipsToPlace.Count 
        ? _shipsToPlace[_currentShipIndex] 
        : 0;
    
    private int _totalShipCells = 20; // 4 + 3 + 3 + 2 + 2 + 2 + 1 + 1 + 1 + 1 = 20 клеток
    

    public ShipPlacementViewModel(
        INavigationService navigationService,
        IGameService gameService,
        GameMode gameMode,
        BotDifficulty botDifficulty)
    {
        _navigationService = navigationService;
        _gameService = gameService;
        _gameMode = gameMode;
        _botDifficulty = botDifficulty;

        _gameService.StartNewGame(_gameMode, _botDifficulty);
        
        // Подписываемся на событие изменения досок
        _gameService.BoardsChanged += OnBoardsChanged;
        
        UpdatePlacementBoard();
        UpdateStatus();
    }

    private void OnBoardsChanged()
    {
        UpdatePlacementBoard();
    }

    private void UpdatePlacementBoard()
    {
        PlacementBoard = _gameService.PlayerBoard;
    }

    [RelayCommand]
    private void RotateShip()
    {
        IsHorizontal = !IsHorizontal;
    }
    
    [ObservableProperty]
    private int _placementBoardVersion = 0;
    
    private int _placedShipsCount = 0;

    [RelayCommand]
    private void PlaceShip((int x, int y) coords)
    {
        if (AllShipsPlaced)
            return;

        if (_gameService.PlaceShip(coords.x, coords.y, CurrentShipSize, IsHorizontal))
        {
            _currentShipIndex++;
            _placedShipsCount++;
    
            AllShipsPlaced = _currentShipIndex >= _shipsToPlace.Count;
    
            UpdateStatus();
        
            // Увеличиваем версию доски для принудительной перерисовки
            PlacementBoardVersion++;
        
            StartGameCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void PlaceShipsRandomly()
    {
        _gameService.PlaceShipsRandomly(true);
        _currentShipIndex = _shipsToPlace.Count;
        _placedShipsCount = 20;
        AllShipsPlaced = true;
        UpdateStatus();
    
        // Увеличиваем версию доски
        PlacementBoardVersion++;
    
        StartGameCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanStartGame))]
    private void StartGame()
    {
        if (_gameMode == GameMode.VsComputer)
        {
            // Размещаем корабли компьютеру
            _gameService.PlaceShipsRandomly(false);
        }

        // Отписываемся от события перед переходом
        _gameService.BoardsChanged -= OnBoardsChanged;

        // Переходим к игровому экрану
        var gameViewModel = new GameViewModel(_navigationService, _gameService);
        _navigationService.NavigateTo(gameViewModel);
    }

    private bool CanStartGame() => AllShipsPlaced;

    private void UpdateStatus()
    {
        if (AllShipsPlaced)
        {
            StatusText = "✅ Все корабли размещены! Нажмите 'Начать игру'";
        }
        else
        {
            StatusText = $"Размещаем корабль размером {CurrentShipSize} клеток " +
                        $"({_currentShipIndex + 1}/{_shipsToPlace.Count})";
        }
    }

    [RelayCommand]
    private void Back()
    {
        // Отписываемся от события перед возвратом
        _gameService.BoardsChanged -= OnBoardsChanged;
        _navigationService.NavigateTo<MenuViewModel>();
    }

    public override void Dispose()
    {
        // Отписываемся от события при уничтожении ViewModel
        _gameService.BoardsChanged -= OnBoardsChanged;
        base.Dispose();
    }
}