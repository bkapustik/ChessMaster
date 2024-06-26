﻿using ChessMaster.Space.Coordinations;
using Emgu.CV.Dnn;

namespace ChessMaster.ChessDriver.ChessMoves;

public class ChessMove
{
    protected readonly SpacePosition source;
    protected readonly SpacePosition target;
    public bool IsEndOfGame { get; set; }
    public bool StateUpdateOnly { get; set; } = false;
    public string? Message { get; set; }

    public ChessMove(bool isEndOfGame, string? message = null)
    {
        IsEndOfGame = isEndOfGame;
        Message = message;
    }

    public ChessMove(SpacePosition source, SpacePosition target, bool isEndOfGame, string? message = null)
    {
        this.source = source;
        this.target = target;
        IsEndOfGame = isEndOfGame;
        Message = message;
    }

    public virtual void Execute(ChessRobot robot)
    {
        robot.MoveFigureTo(source, target, StateUpdateOnly);
    }

    public virtual string ToUci()
    {
        var source = this.source.ToUci();
        var target = this.target.ToUci();

        return $"{source}{target}";
    }
}