using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipGame
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }

    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }

    public enum CellState
    {
        Empty,
        Ship,
        Miss,
        Hit,
        Sunk
    }

    public class Ship
    {
        public int Size { get; set; }
        public List<(int X, int Y)> Positions { get; set; }
        public int HitCount { get; set; }
        public bool IsHorizontal { get; set; }
        public bool IsSunk => HitCount >= Size;

        public Ship(int size, bool horizontal)
        {
            Size = size;
            IsHorizontal = horizontal;
            Positions = new List<(int, int)>();
            HitCount = 0;
        }
    }

    public class GameBoard
    {
        public CellState[,] Grid { get; private set; }
        public List<Ship> Ships { get; private set; }
        public int Size { get; private set; }

        public GameBoard(int size = 10)
        {
            Size = size;
            Grid = new CellState[size, size];
            Ships = new List<Ship>();
        }

        public bool PlaceShip(Ship ship, int x, int y)
        {
            if (!CanPlaceShip(x, y, ship.Size, ship.IsHorizontal))
                return false;

            for (int i = 0; i < ship.Size; i++)
            {
                int px = ship.IsHorizontal ? x + i : x;
                int py = ship.IsHorizontal ? y : y + i;
                Grid[px, py] = CellState.Ship;
                ship.Positions.Add((px, py));
            }

            Ships.Add(ship);
            return true;
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int px = horizontal ? x + i : x;
                int py = horizontal ? y : y + i;

                if (px >= Size || py >= Size)
                    return false;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int checkX = px + dx;
                        int checkY = py + dy;

                        if (checkX >= 0 && checkX < Size && checkY >= 0 && checkY < Size)
                        {
                            if (Grid[checkX, checkY] == CellState.Ship)
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        public void PlaceShipsRandomly()
        {
            var random = new Random();
            int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

            foreach (var size in shipSizes)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 1000)
                {
                    int x = random.Next(Size);
                    int y = random.Next(Size);
                    bool horizontal = random.Next(2) == 0;

                    var ship = new Ship(size, horizontal);
                    placed = PlaceShip(ship, x, y);
                    attempts++;
                }
            }
        }

        public (bool hit, bool sunk, bool gameOver) Attack(int x, int y)
        {
            if (Grid[x, y] == CellState.Hit || Grid[x, y] == CellState.Miss || Grid[x, y] == CellState.Sunk)
                return (false, false, false);

            if (Grid[x, y] == CellState.Ship)
            {
                Grid[x, y] = CellState.Hit;

                var ship = Ships.FirstOrDefault(s => s.Positions.Contains((x, y)));
                if (ship != null)
                {
                    ship.HitCount++;

                    if (ship.IsSunk)
                    {
                        foreach (var pos in ship.Positions)
                        {
                            Grid[pos.X, pos.Y] = CellState.Sunk;
                        }

                        bool gameOver = Ships.All(s => s.IsSunk);
                        return (true, true, gameOver);
                    }
                }

                return (true, false, false);
            }
            else
            {
                Grid[x, y] = CellState.Miss;
                return (false, false, false);
            }
        }
    }

    public class MainWindow : Window
    {
        private GameBoard playerBoard;
        private GameBoard computerBoard;
        private Canvas playerCanvas;
        private Canvas computerCanvas;
        private TextBlock statusText;
        private TextBlock playerStatsText;
        private TextBlock computerStatsText;
        private bool playerTurn = true;
        private Random random = new Random();
        private int playerHits = 0;
        private int playerMisses = 0;
        private int computerHits = 0;
        private int computerMisses = 0;

        public MainWindow()
        {
            Title = "⚓ Морской бой";
            Width = 1000;
            Height = 700;
            Background = new SolidColorBrush(Color.FromRgb(20, 30, 50));

            InitializeGame();
            BuildUI();
        }

        private void InitializeGame()
        {
            playerBoard = new GameBoard();
            computerBoard = new GameBoard();

            playerBoard.PlaceShipsRandomly();
            computerBoard.PlaceShipsRandomly();

            playerHits = 0;
            playerMisses = 0;
            computerHits = 0;
            computerMisses = 0;
        }

        private void BuildUI()
        {
            var mainPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };

            // Заголовок
            var titlePanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 50, 80)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20, 15),
                Margin = new Thickness(0, 0, 0, 20)
            };

            statusText = new TextBlock
            {
                Text = "⚔️ ВАШ ХОД! Атакуйте поле противника",
                FontSize = 22,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            titlePanel.Child = statusText;

            var boardsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 50
            };

            // Панель игрока
            var playerPanel = new StackPanel { Spacing = 10 };
            
            var playerHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 120, 80)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 8)
            };
            var playerLabel = new TextBlock
            {
                Text = "🛡️ ВАШЕ ПОЛЕ",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            playerHeader.Child = playerLabel;

            playerCanvas = CreateBoardCanvas(playerBoard, false);
            
            playerStatsText = new TextBlock
            {
                Text = "🎯 Статистика: 0 попаданий, 0 промахов",
                FontSize = 14,
                Foreground = Brushes.LightGray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            playerPanel.Children.Add(playerHeader);
            playerPanel.Children.Add(playerCanvas);
            playerPanel.Children.Add(playerStatsText);

            // Панель компьютера
            var computerPanel = new StackPanel { Spacing = 10 };
            
            var computerHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(120, 40, 40)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15, 8)
            };
            var computerLabel = new TextBlock
            {
                Text = "🎯 ПОЛЕ ПРОТИВНИКА",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            computerHeader.Child = computerLabel;

            computerCanvas = CreateBoardCanvas(computerBoard, true);
            
            computerStatsText = new TextBlock
            {
                Text = "💣 Статистика противника: 0 попаданий, 0 промахов",
                FontSize = 14,
                Foreground = Brushes.LightGray,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            computerPanel.Children.Add(computerHeader);
            computerPanel.Children.Add(computerCanvas);
            computerPanel.Children.Add(computerStatsText);

            boardsPanel.Children.Add(playerPanel);
            boardsPanel.Children.Add(computerPanel);

            // Кнопка новой игры
            var resetButton = new Button
            {
                Content = "🔄 НОВАЯ ИГРА",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Padding = new Thickness(30, 12),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(60, 90, 140)),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(8)
            };
            resetButton.Click += (s, e) => ResetGame();

            mainPanel.Children.Add(titlePanel);
            mainPanel.Children.Add(boardsPanel);
            mainPanel.Children.Add(resetButton);

            Content = mainPanel;
        }

        private Canvas CreateBoardCanvas(GameBoard board, bool isEnemy)
        {
            var canvas = new Canvas
            {
                Width = 420,
                Height = 420,
                Background = new SolidColorBrush(Color.FromRgb(30, 50, 80))
            };

            int cellSize = 40;
            int padding = 10;

            // Координаты
            for (int i = 0; i < board.Size; i++)
            {
                var letterText = new TextBlock
                {
                    Text = ((char)('А' + i)).ToString(),
                    FontSize = 14,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.LightGray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                Canvas.SetLeft(letterText, padding + i * cellSize + cellSize / 2 - 5);
                Canvas.SetTop(letterText, 0);
                canvas.Children.Add(letterText);

                var numberText = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 14,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.LightGray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                Canvas.SetLeft(numberText, 0);
                Canvas.SetTop(numberText, padding + i * cellSize + cellSize / 2 - 7);
                canvas.Children.Add(numberText);
            }

            // Отрисовка клеток
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    var cell = CreateCell(board, i, j, cellSize, isEnemy);
                    Canvas.SetLeft(cell, padding + i * cellSize);
                    Canvas.SetTop(cell, padding + j * cellSize);
                    canvas.Children.Add(cell);
                }
            }

            return canvas;
        }

        private Control CreateCell(GameBoard board, int x, int y, int cellSize, bool isEnemy)
        {
            var border = new Border
            {
                Width = cellSize - 2,
                Height = cellSize - 2,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 90, 120)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Background = GetCellBrush(board.Grid[x, y], isEnemy),
                BoxShadow = new BoxShadows(new BoxShadow
                {
                    OffsetX = 0,
                    OffsetY = 2,
                    Blur = 4,
                    Color = Color.FromArgb(100, 0, 0, 0)
                })
            };

            var content = new Canvas { Width = cellSize - 2, Height = cellSize - 2 };

            // Добавляем визуальные элементы в зависимости от состояния
            if (board.Grid[x, y] == CellState.Ship && !isEnemy)
            {
                DrawShipSegment(content, cellSize - 2);
            }
            else if (board.Grid[x, y] == CellState.Miss)
            {
                DrawMiss(content, cellSize - 2);
            }
            else if (board.Grid[x, y] == CellState.Hit)
            {
                DrawHit(content, cellSize - 2);
            }
            else if (board.Grid[x, y] == CellState.Sunk)
            {
                DrawSunk(content, cellSize - 2);
            }

            border.Child = content;

            if (isEnemy)
            {
                int cx = x, cy = y;
                border.PointerPressed += (s, e) => OnCellClick(cx, cy);
                border.Cursor = new Cursor(StandardCursorType.Hand);
                
                border.PointerEntered += (s, e) =>
                {
                    if (board.Grid[cx, cy] == CellState.Empty || board.Grid[cx, cy] == CellState.Ship)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(80, 110, 150));
                    }
                };
                
                border.PointerExited += (s, e) =>
                {
                    border.Background = GetCellBrush(board.Grid[cx, cy], isEnemy);
                };
            }

            return border;
        }

        private void DrawShipSegment(Canvas canvas, int size)
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

        private void DrawMiss(Canvas canvas, int size)
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

        private void DrawHit(Canvas canvas, int size)
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

        private void DrawSunk(Canvas canvas, int size)
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

        private Brush GetCellBrush(CellState state, bool isEnemy)
        {
            return state switch
            {
                CellState.Empty => new SolidColorBrush(Color.FromRgb(50, 80, 120)),
                CellState.Ship => isEnemy 
                    ? new SolidColorBrush(Color.FromRgb(50, 80, 120))
                    : new SolidColorBrush(Color.FromRgb(70, 100, 140)),
                CellState.Miss => new SolidColorBrush(Color.FromRgb(60, 100, 150)),
                CellState.Hit => new SolidColorBrush(Color.FromRgb(180, 120, 40)),
                CellState.Sunk => new SolidColorBrush(Color.FromRgb(150, 50, 50)),
                _ => new SolidColorBrush(Color.FromRgb(50, 80, 120))
            };
        }

        private void OnCellClick(int x, int y)
        {
            if (!playerTurn) return;

            var (hit, sunk, gameOver) = computerBoard.Attack(x, y);

            if (hit)
            {
                playerHits++;
                if (sunk)
                {
                    statusText.Text = gameOver 
                        ? "🎉 ПОБЕДА! Вы потопили весь флот противника!" 
                        : "💥 Корабль потоплен! Продолжайте атаку!";
                }
                else
                {
                    statusText.Text = "🔥 ПОПАДАНИЕ! Атакуйте снова!";
                }

                if (gameOver)
                {
                    playerTurn = false;
                }
            }
            else if (computerBoard.Grid[x, y] == CellState.Miss)
            {
                playerMisses++;
                statusText.Text = "💧 Промах! Ход переходит к противнику...";
                playerTurn = false;
                
                var timer = new System.Threading.Timer(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(ComputerTurn);
                }, null, 800, System.Threading.Timeout.Infinite);
            }

            UpdateStats();
            UpdateBoard(computerCanvas, computerBoard, true);
        }

        private void ComputerTurn()
        {
            bool continueTurn = true;

            while (continueTurn && !playerTurn)
            {
                int x = random.Next(playerBoard.Size);
                int y = random.Next(playerBoard.Size);

                var (hit, sunk, gameOver) = playerBoard.Attack(x, y);

                if (hit || playerBoard.Grid[x, y] == CellState.Miss)
                {
                    if (hit)
                    {
                        computerHits++;
                        if (sunk)
                        {
                            statusText.Text = gameOver 
                                ? "💀 ПОРАЖЕНИЕ! Противник уничтожил ваш флот!" 
                                : "⚠️ Противник потопил ваш корабль!";
                        }
                        else
                        {
                            statusText.Text = "💥 Противник попал в ваш корабль!";
                        }

                        if (gameOver)
                        {
                            continueTurn = false;
                        }
                    }
                    else
                    {
                        computerMisses++;
                        statusText.Text = "⚔️ Противник промахнулся! ВАШ ХОД!";
                        playerTurn = true;
                        continueTurn = false;
                    }

                    UpdateStats();
                    UpdateBoard(playerCanvas, playerBoard, false);
                    break;
                }
            }
        }

        private void UpdateStats()
        {
            playerStatsText.Text = $"🎯 Ваши выстрелы: {playerHits} попаданий, {playerMisses} промахов";
            computerStatsText.Text = $"💣 Выстрелы противника: {computerHits} попаданий, {computerMisses} промахов";
        }

        private void UpdateBoard(Canvas canvas, GameBoard board, bool isEnemy)
        {
            canvas.Children.Clear();
            
            int cellSize = 40;
            int padding = 10;

            // Координаты
            for (int i = 0; i < board.Size; i++)
            {
                var letterText = new TextBlock
                {
                    Text = ((char)('А' + i)).ToString(),
                    FontSize = 14,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.LightGray
                };
                Canvas.SetLeft(letterText, padding + i * cellSize + cellSize / 2 - 5);
                Canvas.SetTop(letterText, 0);
                canvas.Children.Add(letterText);

                var numberText = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 14,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.LightGray
                };
                Canvas.SetLeft(numberText, 0);
                Canvas.SetTop(numberText, padding + i * cellSize + cellSize / 2 - 7);
                canvas.Children.Add(numberText);
            }

            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    var cell = CreateCell(board, i, j, cellSize, isEnemy);
                    Canvas.SetLeft(cell, padding + i * cellSize);
                    Canvas.SetTop(cell, padding + j * cellSize);
                    canvas.Children.Add(cell);
                }
            }
        }

        private void ResetGame()
        {
            InitializeGame();
            playerTurn = true;
            statusText.Text = "⚔️ ВАШ ХОД! Атакуйте поле противника";
            UpdateStats();
            UpdateBoard(playerCanvas, playerBoard, false);
            UpdateBoard(computerCanvas, computerBoard, true);
        }
    }
}
