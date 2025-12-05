using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BattleShipGame2.Converters;

public static class StringConverters
{
    public static readonly IValueConverter IsNotNullOrEmpty = 
        new FuncValueConverter<string?, bool>(value => !string.IsNullOrEmpty(value));
}