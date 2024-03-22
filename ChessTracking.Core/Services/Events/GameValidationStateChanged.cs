using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Services.Events;

public delegate void GameValidationStateChangedEvent(object? o, GameValidationStateChangedEventArgs e);

public class GameValidationStateChangedEventArgs : EventArgs
{
    public bool? IsValid { get; set; }
    public GameValidationStateChangedEventArgs(bool isValid)
    {
        IsValid = isValid;
    }
}
