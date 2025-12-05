using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleShipGame2.Views;

namespace BattleShipGame2.Networking;

public class ChatManager
{
    #region Поля и свойства
    private NetworkClient _networkClient;
    private string _playerName;
    private ChatControl? _chatControl;

    public event Action<string, string, DateTime>? MessageAdded;

    /// <summary>
    /// Инициализация чат-менеджера.
    /// </summary>
    public ChatManager(NetworkClient networkClient, string playerName)
    {
        _networkClient = networkClient;
        _playerName = playerName;
    }
    
    #endregion
    
    #region Основная логика
    
    /// <summary>
    /// Создаёт и возвращает готовый ChatControl
    /// </summary>
    public ChatControl CreateChatControl()
    {
        _chatControl = new ChatControl();
        _chatControl.SetChatManager(this);
        return _chatControl;
    }

    /// <summary>
    /// Добавляет сообщение в чат
    /// </summary>
    public void AddMessage(string sender, string text, DateTime timestamp)
    {
        Console.WriteLine($"[ChatManager] AddMessage called: {sender}: {text}");
        MessageAdded?.Invoke(sender, text, timestamp);
    }

    /// <summary>
    /// Обрабатывает входящее сообщение чата
    /// </summary>
    public void HandleChatMessage(Dictionary<string, string> data)
    {
        var sender = data.GetValueOrDefault(NetworkProtocol.Keys.Sender, "Opponent");
        var text = data.GetValueOrDefault(NetworkProtocol.Keys.ChatText, "");

        Console.WriteLine($"[Chat] {sender}: {text}");
        if (MessageAdded == null)
        {
            Console.WriteLine($"[ChatManager] WARNING: MessageAdded event has no subscribers!");
        }
        else
        {
            Console.WriteLine($"[ChatManager] MessageAdded event has subscribers, invoking...");
        }
        AddMessage(sender, text, DateTime.Now);
    }
    
    /// <summary>
    /// Отправляет сообщение в чат сопернику
    /// </summary>
    public async Task SendChatMessageAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || !_networkClient.IsConnected)
            return;
    
        var chatMsg = new NetworkMessage
        {
            Type = NetworkProtocol.Commands.ChatMessage,
            Data = { { NetworkProtocol.Keys.ChatText, text } }
        };
    
        await _networkClient.SendMessageAsync(chatMsg);
    
        // Добавляем своё сообщение в чат
        AddMessage("Вы", text, DateTime.Now);
    }

    /// <summary>
    /// Очищает историю сообщений
    /// </summary>
    public void Clear()
    {
        _chatControl?.Clear();
    }
    
    
    
    #endregion
}