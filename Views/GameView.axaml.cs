// Views/GameView.xaml.cs
using System;
using System.ComponentModel;
using Avalonia.Controls;
using BattleShipGame2.ViewModels;
using BattleShipGame2.Helpers;

namespace BattleShipGame2.Views;

public partial class GameView : UserControl
{
    private GameViewModel? _viewModel;

    public GameView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (DataContext is GameViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            RenderBoards();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameViewModel.PlayerBoard) ||
            e.PropertyName == nameof(GameViewModel.OpponentBoard))
        {
            RenderBoards();
        }
    }

    
    private void RenderBoards()
    {
        Console.WriteLine($"RenderBoards called: PlayerBoard={_viewModel?.PlayerBoard}, OpponentBoard={_viewModel?.OpponentBoard}");
        Console.WriteLine($"CanAttack: {_viewModel?.CanAttack}");

        if (_viewModel == null || OwnCanvas == null || EnemyCanvas == null)
        {
            Console.WriteLine("Something is null!");
            return;
        }

        // Проверяем стили
        Console.WriteLine($"OwnCanvas Background: {OwnCanvas.Background}");
        Console.WriteLine($"OwnCanvas Width/Height: {OwnCanvas.Width}/{OwnCanvas.Height}");

        // Отрисовываем своё поле (с видимыми кораблями)
        if (_viewModel.PlayerBoard != null)
        {
            Console.WriteLine("Rendering player board...");
            BoardRenderer.RenderBoard(
                OwnCanvas,
                _viewModel.PlayerBoard,
                isEnemy: false,
                onCellClick: null);
        }

        // Отрисовываем поле противника (корабли скрыты)
        if (_viewModel.OpponentBoard != null)
        {
            Console.WriteLine($"Rendering enemy board... CanAttack={_viewModel.CanAttack}");
            BoardRenderer.RenderBoard(
                EnemyCanvas,
                _viewModel.OpponentBoard,
                isEnemy: true,
                onCellClick: _viewModel.CanAttack ? OnEnemyCellClick : null);
        }
    }

    private void OnEnemyCellClick(int x, int y)
    {
        Console.WriteLine($"OnEnemyCellClick: ({x},{y})");
        _viewModel?.AttackCommand.Execute((x, y));
    }
    
}