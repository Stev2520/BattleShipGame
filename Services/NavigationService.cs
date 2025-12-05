using System;
using BattleShipGame2.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BattleShipGame2.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase? _currentViewModel;
    
    public event Action<ViewModelBase>? CurrentViewModelChanged;
    
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel ??= GetInitialViewModel();
        private set
        {
            // Освобождаем предыдущую ViewModel
            _currentViewModel?.Dispose();
            
            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke(_currentViewModel);
        }
    }
    
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    private ViewModelBase GetInitialViewModel()
    {
        return _serviceProvider.GetRequiredService<MenuViewModel>();
    }
    
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModel = viewModel;
    }
    
    public void NavigateTo(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
    }
}