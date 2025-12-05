/*// ViewModels/MainWindowViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using BattleShipGame2.Services;

namespace BattleShipGame2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        // Подписываемся на изменение CurrentViewModel
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Инициализируем текущую ViewModel
        CurrentViewModel = _navigationService.CurrentViewModel;
    }

    private void OnCurrentViewModelChanged(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }
}*/
namespace BattleShipGame2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";
}