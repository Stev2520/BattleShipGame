using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BattleShipGame2.Models;

namespace BattleShipGame2.Views;

public partial class DifficultyWindow : Window
{
    public BotDifficulty? SelectedDifficulty { get; private set; }
    
    public DifficultyWindow()
    {
        InitializeComponent();
        
        // Находим кнопки
        var easyButton = this.FindControl<Button>("EasyDifficultyButton");
        var mediumButton = this.FindControl<Button>("MediumDifficultyButton");
        var hardButton = this.FindControl<Button>("HardDifficultyButton");
        
        // Обработчики событий
        easyButton.Click += (s, e) => SelectDifficulty(BotDifficulty.Easy);
        mediumButton.Click += (s, e) => SelectDifficulty(BotDifficulty.Medium);
        hardButton.Click += (s, e) => SelectDifficulty(BotDifficulty.Hard);
    }
    
    private void SelectDifficulty(BotDifficulty difficulty)
    {
        SelectedDifficulty = difficulty;
        Close();
    }
}