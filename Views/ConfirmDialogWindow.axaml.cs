using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BattleShipGame2.Views;

public partial class ConfirmDialogWindow : Window
{
    private TextBlock messageText;
    
    public string Message 
    { 
        get => messageText?.Text ?? string.Empty;
        set 
        { 
            if (messageText != null) 
                messageText.Text = value; 
        }
    }
    
    public ConfirmDialogWindow()
    {
        InitializeComponent();
        
        // Находим элементы
        messageText = this.FindControl<TextBlock>("ConfirmDialogMessage");
        var yesButton = this.FindControl<Button>("ConfirmYesButton");
        var noButton = this.FindControl<Button>("ConfirmNoButton");
        
        // Обработчики кнопок
        yesButton.Click += (s, e) => Close(true);
        noButton.Click += (s, e) => Close(false);
    }
}