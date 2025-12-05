using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BattleShipGame2.Services;
using BattleShipGame2.Models;

namespace BattleShipGame2.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IGameService _gameService;

    public MenuViewModel(INavigationService navigationService, IGameService gameService)
    {
        _navigationService = navigationService;
        _gameService = gameService;
    }

    [RelayCommand]
    private void PlayVsComputer()
    {
        _navigationService.NavigateTo<DifficultySelectionViewModel>();
    }

    [RelayCommand]
    private void PlayVsPlayer()
    {
        // Используем сервис провайдер для создания ViewModel
        var shipPlacementViewModel = new ShipPlacementViewModel(
            _navigationService,
            _gameService,
            GameMode.VsPlayer,
            BotDifficulty.Easy);
        
        _navigationService.NavigateTo(shipPlacementViewModel);
    }

    [RelayCommand]
    private void PlayOnline()
    {
        _navigationService.NavigateTo<NetworkConnectionViewModel>();
    }
}