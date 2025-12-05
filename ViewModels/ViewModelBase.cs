using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BattleShipGame2.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    public virtual void Dispose()
    {
        // Базовая реализация для переопределения в наследниках
    }
}