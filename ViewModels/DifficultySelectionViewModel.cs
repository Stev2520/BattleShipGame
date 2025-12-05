using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BattleShipGame2.Services;
using BattleShipGame2.Logic;
using BattleShipGame2.Models;

namespace BattleShipGame2.ViewModels;

public partial class DifficultySelectionViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IGameService _gameService;

    public DifficultySelectionViewModel(
        INavigationService navigationService, 
        IGameService gameService)
    {
        _navigationService = navigationService;
        _gameService = gameService;
    }

    [RelayCommand]
    private void SelectDifficulty(string difficulty)
    {
        var botDifficulty = difficulty switch
        {
            "Easy" => BotDifficulty.Easy,
            "Medium" => BotDifficulty.Medium,
            "Hard" => BotDifficulty.Hard,
            _ => BotDifficulty.Easy
        };

        // Создаем ShipPlacementViewModel вручную
        var viewModel = new ShipPlacementViewModel(
            _navigationService,
            _gameService,
            GameMode.VsComputer,
            botDifficulty);
        
        _navigationService.NavigateTo(viewModel);
    }

    [RelayCommand]
    private void Back()
    {
        _navigationService.NavigateTo<MenuViewModel>();
    }
}