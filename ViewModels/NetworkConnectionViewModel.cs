// ViewModels/NetworkConnectionViewModel.cs
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BattleShipGame2.Services;

namespace BattleShipGame2.ViewModels;

public partial class NetworkConnectionViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _playerName = "";

    [ObservableProperty]
    private string _serverAddress = "127.0.0.1";

    [ObservableProperty]
    private string _serverPort = "8889";

    [ObservableProperty]
    private string _errorMessage = "";

    public NetworkConnectionViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task Connect()
    {
        // TODO: Implement network connection logic
        ErrorMessage = "Сетевой режим в разработке";
        await Task.Delay(100); // Просто чтобы убрать warning
    }

    [RelayCommand]
    private void Back()
    {
        _navigationService.NavigateTo<MenuViewModel>();
    }
}