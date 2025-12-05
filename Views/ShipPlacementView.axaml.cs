using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using BattleShipGame2.ViewModels;
using BattleShipGame2.Helpers;

namespace BattleShipGame2.Views;

public partial class ShipPlacementView : UserControl
{
    private ShipPlacementViewModel? _viewModel;

    public ShipPlacementView()
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

        if (DataContext is ShipPlacementViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            RenderBoard();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Console.WriteLine($"ShipPlacementView: Property changed: {e.PropertyName}");
    
        if (e.PropertyName == nameof(ShipPlacementViewModel.PlacementBoard) ||
            e.PropertyName == nameof(ShipPlacementViewModel.CurrentShipIndex) ||
            e.PropertyName == nameof(ShipPlacementViewModel.IsHorizontal) ||
            e.PropertyName == nameof(ShipPlacementViewModel.PlacementBoardVersion))
        {
            RenderBoard();
        }
    }

    private void RenderBoard()
    {
        Console.WriteLine($"ShipPlacementView.RenderBoard called");
    
        if (_viewModel == null || PlacementCanvas == null)
            return;

        BoardRenderer.RenderPlacementBoard(
            PlacementCanvas,
            _viewModel.PlacementBoard,
            OnCellClick,
            CanPlaceShipAt);
    }

    private void OnCellClick(int x, int y)
    {
        _viewModel?.PlaceShipCommand.Execute((x, y));
    }

    private bool CanPlaceShipAt(int x, int y)
    {
        if (_viewModel == null || _viewModel.CurrentShipSize == 0)
            return false;

        // Проверяем, можно ли разместить корабль
        var board = _viewModel.PlacementBoard;
        int shipSize = _viewModel.CurrentShipSize;
        bool horizontal = _viewModel.IsHorizontal;

        // Проверка границ
        if (horizontal && x + shipSize > board.Size)
            return false;
        if (!horizontal && y + shipSize > board.Size)
            return false;

        // Проверка занятости клеток
        for (int i = 0; i < shipSize; i++)
        {
            int checkX = horizontal ? x + i : x;
            int checkY = horizontal ? y : y + i;

            if (board.Grid[checkX, checkY] != Models.CellState.Empty)
                return false;

            // Проверка соседних клеток
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = checkX + dx;
                    int ny = checkY + dy;

                    if (nx >= 0 && nx < board.Size && ny >= 0 && ny < board.Size)
                    {
                        if (board.Grid[nx, ny] == Models.CellState.Ship)
                            return false;
                    }
                }
            }
        }

        return true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
        {
            _viewModel?.RotateShipCommand.Execute(null);
        }
    }
}