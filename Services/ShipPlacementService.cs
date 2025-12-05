/*// Services/ShipPlacementService.cs
using BattleShipGame2.Models;
using System.Collections.Generic;

namespace BattleShipGame2.Services;

public class ShipPlacementService
{
    private List<int> _shipsToPlace = new() { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
    private int _currentShipIndex = 0;
    private bool _currentShipHorizontal = true;
    
    public List<int> ShipsToPlace => _shipsToPlace;
    public int CurrentShipIndex => _currentShipIndex;
    public bool CurrentShipHorizontal => _currentShipHorizontal;
    public int CurrentShipSize => _currentShipIndex < _shipsToPlace.Count ? _shipsToPlace[_currentShipIndex] : 0;
    public bool AllShipsPlaced => _currentShipIndex >= _shipsToPlace.Count;
    
    public void Reset()
    {
        _currentShipIndex = 0;
        _currentShipHorizontal = true;
    }
    
    public void RotateShip()
    {
        _currentShipHorizontal = !_currentShipHorizontal;
    }
    
    public bool PlaceShip(GameBoard board, int x, int y)
    {
        if (_currentShipIndex >= _shipsToPlace.Count)
            return false;
            
        int shipSize = _shipsToPlace[_currentShipIndex];
        var ship = new Ship(shipSize, _currentShipHorizontal);
        
        if (board.PlaceShip(ship, x, y))
        {
            _currentShipIndex++;
            return true;
        }
        
        return false;
    }
    
    public void PlaceShipsRandomly(GameBoard board)
    {
        board.PlaceShipsRandomly();
        _currentShipIndex = _shipsToPlace.Count;
    }
    
    public bool CanPlaceShip(GameBoard board, int x, int y)
    {
        if (_currentShipIndex >= _shipsToPlace.Count)
            return false;
            
        int shipSize = _shipsToPlace[_currentShipIndex];
        return board.CanPlaceShip(x, y, shipSize, _currentShipHorizontal);
    }
}*/