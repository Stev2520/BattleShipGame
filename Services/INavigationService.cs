using System;
using BattleShipGame2.ViewModels;

namespace BattleShipGame2.Services;

public interface INavigationService
{
    event Action<ViewModelBase>? CurrentViewModelChanged;
    ViewModelBase CurrentViewModel { get; }
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo(ViewModelBase viewModel);
}