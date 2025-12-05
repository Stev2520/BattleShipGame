using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using BattleShipGame2.Models;

namespace BattleShipGame2.Helpers;

/// <summary>
/// Вспомогательный класс для отрисовки игровых досок на Canvas
/// </summary>
public static class BoardRenderer
{
    private const int CellSize = 40;
    private const int Padding = 10;

    /// <summary>
    /// Отрисовывает игровую доску на Canvas
    /// </summary>
    public static void RenderBoard(Canvas canvas, GameBoard board, bool isEnemy, Action<int, int>? onCellClick = null)
    {
        Console.WriteLine($"BoardRenderer.RenderBoard called: canvas={canvas}, board={board}, isEnemy={isEnemy}");
    
        canvas.Children.Clear();
    
        // Явно устанавливаем размеры
        canvas.Width = 420;
        canvas.Height = 420;
    
        // Устанавливаем фон Canvas
        canvas.Background = new SolidColorBrush(Color.FromArgb(255, 30, 50, 80)); // #1E3250
        Console.WriteLine($"Canvas background set to: {canvas.Background}");
    
        // Рисуем координаты
        RenderCoordinates(canvas);
    
        // Рисуем клетки
        for (int x = 0; x < board.Size; x++)
        {
            for (int y = 0; y < board.Size; y++)
            {
                var cell = CreateCell(board, x, y, isEnemy, onCellClick);
                Canvas.SetLeft(cell, Padding + x * CellSize);
                Canvas.SetTop(cell, Padding + y * CellSize);
                canvas.Children.Add(cell);
            
                // Отладочный вывод для первой клетки
                if (x == 0 && y == 0 && cell is Border border)
                {
                    Console.WriteLine($"First cell classes: {string.Join(", ", border.Classes)}");
                    Console.WriteLine($"First cell background: {border.Background}");
                }
            }
        }
    
        Console.WriteLine($"Canvas children count: {canvas.Children.Count}");
    }

    private static Control CreateCell(GameBoard board, int x, int y, bool isEnemy, Action<int, int>? onCellClick)
    {
        var border = new Border
        {
            Width = CellSize - 2,
            Height = CellSize - 2,
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 90, 120)), // #3C5A78
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 2,
                Blur = 4,
                Color = Color.FromArgb(100, 0, 0, 0)
            })
        };

        var state = board.Grid[x, y];
        
        // Явно устанавливаем фон в зависимости от состояния
        border.Background = GetCellBrush(state, isEnemy);
        
        // Добавляем класс GameCell для стилей
        border.Classes.Add("GameCell");
        
        // Определяем и добавляем класс состояния
        if (isEnemy && state == CellState.Ship)
        {
            // Для вражеского поля скрываем непораженные корабли
            border.Classes.Add("Empty");
        }
        else
        {
            border.Classes.Add(state switch
            {
                CellState.Empty => "Empty",
                CellState.Ship => "Ship",
                CellState.Miss => "Miss",
                CellState.Hit => "Hit",
                CellState.Sunk => "Sunk",
                CellState.Blocked => "Blocked",
                _ => "Empty"
            });
        }

        var content = new Canvas { Width = CellSize - 2, Height = CellSize - 2 };

        // Рисуем содержимое клетки
        switch (state)
        {
            case CellState.Ship when !isEnemy:
                DrawShipSegment(content, CellSize - 2);
                break;
            case CellState.Miss:
                DrawMiss(content, CellSize - 2);
                break;
            case CellState.Hit:
                DrawHit(content, CellSize - 2);
                break;
            case CellState.Sunk:
                DrawSunk(content, CellSize - 2);
                break;
            case CellState.Blocked:
                DrawBlocked(content, CellSize - 2);
                break;
        }

        border.Child = content;

        // Добавляем обработчик клика для вражеского поля
        if (isEnemy && onCellClick != null)
        {
            var cellState = board.Grid[x, y];
            bool canClick = cellState == CellState.Empty || cellState == CellState.Ship;

            if (canClick)
            {
                int capturedX = x;
                int capturedY = y;
                
                Console.WriteLine($"Cell ({capturedX},{capturedY}) - clickable, state={cellState}");
                
                border.PointerPressed += (s, e) => 
                {
                    Console.WriteLine($"Cell ({capturedX},{capturedY}) clicked!");
                    onCellClick(capturedX, capturedY);
                };
                
                border.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);

                // Hover эффект
                border.PointerEntered += (s, e) => border.Opacity = 0.8;
                border.PointerExited += (s, e) => border.Opacity = 1.0;
            }
            else
            {
                Console.WriteLine($"Cell ({x},{y}) - NOT clickable, state={cellState}");
                // Делаем клетку некликабельной, если уже атакована
                border.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
            }
        }

        return border;
    }

    private static IBrush GetCellBrush(CellState state, bool isEnemy)
    {
        // Если это вражеское поле и клетка с кораблем, показываем как пустую
        if (isEnemy && state == CellState.Ship)
        {
            return new SolidColorBrush(Color.FromArgb(255, 50, 80, 120)); // #325078
        }
        
        return state switch
        {
            CellState.Empty => new SolidColorBrush(Color.FromArgb(255, 50, 80, 120)),  // #325078
            CellState.Ship => new SolidColorBrush(Color.FromArgb(255, 70, 100, 140)),   // #46648C
            CellState.Miss => new SolidColorBrush(Color.FromArgb(255, 60, 100, 150)),   // #3C6496
            CellState.Hit => new SolidColorBrush(Color.FromArgb(255, 180, 120, 40)),    // #B47828
            CellState.Sunk => new SolidColorBrush(Color.FromArgb(255, 150, 50, 50)),    // #963232
            CellState.Blocked => new SolidColorBrush(Color.FromArgb(255, 40, 60, 90)),  // #283C5A
            _ => new SolidColorBrush(Color.FromArgb(255, 50, 80, 120))
        };
    }

    /// <summary>
    /// Отрисовывает доску для расстановки кораблей
    /// </summary>
    public static void RenderPlacementBoard(
        Canvas canvas, 
        GameBoard board, 
        Action<int, int>? onCellClick = null,
        Func<int, int, bool>? canPlaceAt = null)
    {
        canvas.Children.Clear();
        canvas.Width = 420;
        canvas.Height = 420;
        canvas.Background = new SolidColorBrush(Color.FromArgb(255, 30, 50, 80));

        // Координаты
        RenderCoordinates(canvas);

        // Клетки
        for (int x = 0; x < board.Size; x++)
        {
            for (int y = 0; y < board.Size; y++)
            {
                var cell = CreatePlacementCell(board, x, y, onCellClick, canPlaceAt);
                Canvas.SetLeft(cell, Padding + x * CellSize);
                Canvas.SetTop(cell, Padding + y * CellSize);
                canvas.Children.Add(cell);
            }
        }
    }
    
    private static void RenderCoordinates(Canvas canvas)
    {
        for (int i = 0; i < 10; i++)
        {
            // Буквы сверху
            var letterText = new TextBlock
            {
                Text = ((char)('А' + i)).ToString(),
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 160, 192, 224)) // #A0C0E0
            };
            Canvas.SetLeft(letterText, Padding + i * CellSize + CellSize / 2 - 5);
            Canvas.SetTop(letterText, 0);
            canvas.Children.Add(letterText);

            // Цифры слева
            var numberText = new TextBlock
            {
                Text = (i + 1).ToString(),
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 160, 192, 224))
            };
            Canvas.SetLeft(numberText, 0);
            Canvas.SetTop(numberText, Padding + i * CellSize + CellSize / 2 - 7);
            canvas.Children.Add(numberText);
        }
    }

    private static Control CreatePlacementCell(
        GameBoard board, 
        int x, 
        int y, 
        Action<int, int>? onCellClick,
        Func<int, int, bool>? canPlaceAt)
    {
        var border = new Border
        {
            Width = CellSize - 2,
            Height = CellSize - 2,
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 90, 120)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };

        // Добавляем класс PlacementCell
        border.Classes.Add("PlacementCell");

        if (board.Grid[x, y] == CellState.Ship)
        {
            border.Classes.Add("Ship");
            border.Background = new SolidColorBrush(Color.FromArgb(255, 70, 100, 140)); // #46648C
            var content = new Canvas { Width = CellSize - 2, Height = CellSize - 2 };
            DrawShipSegment(content, CellSize - 2);
            border.Child = content;
        }
        else
        {
            border.Classes.Add("Empty");
            border.Background = new SolidColorBrush(Color.FromArgb(255, 50, 80, 120)); // #325078
        }

        if (onCellClick != null)
        {
            int capturedX = x;
            int capturedY = y;
            
            border.PointerPressed += (s, e) => 
            {
                Console.WriteLine($"Placement cell ({capturedX},{capturedY}) clicked!");
                onCellClick(capturedX, capturedY);
            };

            // Подсветка при наведении
            if (canPlaceAt != null)
            {
                border.PointerEntered += (s, e) =>
                {
                    bool canPlace = canPlaceAt(capturedX, capturedY);
                    border.Classes.Remove("Empty");
                    if (canPlace)
                    {
                        border.Classes.Add("CanPlace");
                        border.Background = new SolidColorBrush(Color.FromArgb(255, 100, 180, 100)); // #64B464
                    }
                    else
                    {
                        border.Classes.Add("CannotPlace");
                        border.Background = new SolidColorBrush(Color.FromArgb(255, 180, 100, 100)); // #B46464
                    }
                };

                border.PointerExited += (s, e) =>
                {
                    border.Classes.Remove("CanPlace");
                    border.Classes.Remove("CannotPlace");
                    if (board.Grid[capturedX, capturedY] != CellState.Ship)
                    {
                        border.Classes.Add("Empty");
                        border.Background = new SolidColorBrush(Color.FromArgb(255, 50, 80, 120)); // #325078
                    }
                };
            }
        }

        return border;
    }

    // ============================================================
    // Методы отрисовки содержимого клеток
    // ============================================================

    private static void DrawShipSegment(Canvas canvas, int size)
    {
        var ship = new Ellipse
        {
            Width = size * 0.7,
            Height = size * 0.7,
            Fill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(Color.FromRgb(100, 100, 100), 0),
                    new GradientStop(Color.FromRgb(60, 60, 60), 1)
                }
            }
        };
        Canvas.SetLeft(ship, size * 0.15);
        Canvas.SetTop(ship, size * 0.15);
        canvas.Children.Add(ship);
    }

    private static void DrawMiss(Canvas canvas, int size)
    {
        var circle = new Ellipse
        {
            Width = size * 0.3,
            Height = size * 0.3,
            Fill = new SolidColorBrush(Color.FromRgb(100, 150, 200))
        };
        Canvas.SetLeft(circle, size * 0.35);
        Canvas.SetTop(circle, size * 0.35);
        canvas.Children.Add(circle);
    }

    private static void DrawHit(Canvas canvas, int size)
    {
        var line1 = new Line
        {
            StartPoint = new Point(size * 0.2, size * 0.2),
            EndPoint = new Point(size * 0.8, size * 0.8),
            Stroke = Brushes.Yellow,
            StrokeThickness = 3
        };
        var line2 = new Line
        {
            StartPoint = new Point(size * 0.8, size * 0.2),
            EndPoint = new Point(size * 0.2, size * 0.8),
            Stroke = Brushes.Yellow,
            StrokeThickness = 3
        };
        canvas.Children.Add(line1);
        canvas.Children.Add(line2);
    }

    private static void DrawSunk(Canvas canvas, int size)
    {
        var line1 = new Line
        {
            StartPoint = new Point(size * 0.2, size * 0.2),
            EndPoint = new Point(size * 0.8, size * 0.8),
            Stroke = Brushes.Red,
            StrokeThickness = 4
        };
        var line2 = new Line
        {
            StartPoint = new Point(size * 0.8, size * 0.2),
            EndPoint = new Point(size * 0.2, size * 0.8),
            Stroke = Brushes.Red,
            StrokeThickness = 4
        };
        canvas.Children.Add(line1);
        canvas.Children.Add(line2);
    }

    private static void DrawBlocked(Canvas canvas, int size)
    {
        var dot = new Ellipse
        {
            Width = size * 0.15,
            Height = size * 0.15,
            Fill = new SolidColorBrush(Color.FromRgb(80, 100, 130))
        };
        Canvas.SetLeft(dot, size * 0.425);
        Canvas.SetTop(dot, size * 0.425);
        canvas.Children.Add(dot);
    }
}