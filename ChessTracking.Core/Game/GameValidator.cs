using ChessTracking.Core.Tracking.State;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.Game;

/// <summary>
/// Validator of chess game
/// </summary>
static class GameValidator
{
    /// <summary>
    /// Validates and performs move described in text
    /// </summary>
    /// <param name="game">Description of game</param>
    /// <param name="move">Move to process</param>
    /// <returns>Result of validation</returns>
    public static ValidationResult ValidateAndPerform(GameData game, string move)
    {
        return ValidateAndPerform(game, DecodeMove(game, move));
    }

    /// <summary>
    /// Validates and performs move described by difference between game and trackingState
    /// </summary>
    /// <param name="game">Description of game</param>
    /// <param name="move">Tracking state of game</param>
    /// <returns>Result of validation</returns>
    public static ValidationResult ValidateAndPerform(GameData game, TrackingState trackingState)
    {
        return ValidateAndPerform(game, DecodeMove(game, trackingState));
    }

    /// <summary>
    /// Validates and performs given move 
    /// </summary>
    /// <param name="game">Description of game</param>
    /// <returns>Result of validation</returns>
    private static ValidationResult ValidateAndPerform(GameData game, GameMove myMove)
    {
        if (myMove == null)
            return new ValidationResult(false, null);

        var myMoves = GetAllMoves(game.Chessboard, game.PlayerOnMove);

        // move is in possible moves
        if (myMoves.Any(x => x.IsEquivalent(myMove)))
            PerformMove(game, myMove);
        else
            return new ValidationResult(false, null);

        // move can't end in beeing checked
        if (PlayerIsChecked(game, game.PlayerOnMove))
            return new ValidationResult(false, null);

        game.Moves.Add(myMove);
        game.RecordOfGame.Add(SerializeMoveToAlgebraicNotation(myMove));

        // chech whether opponent in in checkmate or stalemate
        if (PlayerHasNoPossibleMoves(game, GetOppositeColor(game.PlayerOnMove)))
        {
            if (PlayerIsChecked(game, GetOppositeColor(game.PlayerOnMove)))
                game.EndState = GetWinStateFromPlayerColor(game.PlayerOnMove);
            else
                game.EndState = GameState.Draw;
        }

        // alternate playing player
        game.PlayerOnMove = GetOppositeColor(game.PlayerOnMove);

        return new ValidationResult(true, game);
    }

    /// <summary>
    /// Serializes move into its algebraic notation representation
    /// </summary>
    /// <param name="move">Move to serialize</param>
    /// <returns>Textual representation of move</returns>
    private static string SerializeMoveToAlgebraicNotation(GameMove move)
    {
        char separator = move.TargetFigure == null ? '-' : 'x';
        return
            $"{GetCharacterForFigure(move.SourceFigure)}{GetCharacterForPosition(move.Source.X)}{move.Source.Y + 1}{separator}{GetCharacterForPosition(move.Target.X)}{move.Target.Y + 1}";
    }

    /// <summary>
    /// Gets character representation for given figure
    /// </summary>
    private static char GetCharacterForFigure(FigureType figure)
    {
        switch (figure)
        {
            case FigureType.Queen:
                return 'Q';
            case FigureType.King:
                return 'K';
            case FigureType.Rook:
                return 'R';
            case FigureType.Knight:
                return 'N';
            case FigureType.Bishop:
                return 'B';
            case FigureType.Pawn:
                return 'P';
            default:
                throw new ArgumentOutOfRangeException(nameof(figure), figure, null);
        }
    }

    /// <summary>
    /// Gets figure type for given character
    /// </summary>
    private static FigureType GetFigureForCharacter(char ch)
    {
        switch (ch)
        {
            case 'Q':
                return FigureType.Queen;
            case 'K':
                return FigureType.King;
            case 'R':
                return FigureType.Rook;
            case 'N':
                return FigureType.Knight;
            case 'B':
                return FigureType.Bishop;
            case 'P':
                return FigureType.Pawn;
            default:
                throw new ArgumentOutOfRangeException(nameof(ch), ch, null);
        }
    }

    /// <summary>
    /// Gets character represenation of column on chessboard
    /// </summary>
    /// <param name="position">Integer represenation of column</param>
    private static char GetCharacterForPosition(int position)
    {
        return (char)((int)'a' + position);
    }

    /// <summary>
    /// Gets integer represenation of column on chessboard
    /// </summary>
    /// <param name="ch">Character represenation of column</param>
    private static int GetPositionForCharacter(char ch)
    {
        return (int)((int)ch - (int)'a');
    }

    /// <summary>
    /// Performs given move on chessboard
    /// </summary>
    /// <param name="game">Description of game</param>
    /// <param name="move">Move to perform</param>
    private static void PerformMove(GameData game, GameMove move)
    {
        game.Chessboard.GetFigureOnPosition(move.Source).Moved = true;
        game.Chessboard.MoveTo(move.Source, move.Target);

        if (move.CastlingRookSource != null && move.CastlingRookTarget != null)
        { 
            game.Chessboard.GetFigureOnPosition(move.CastlingRookSource).Moved = true;
            game.Chessboard.MoveTo(move.CastlingRookSource, move.CastlingRookTarget);
        }
    }

    /// <summary>
    /// Reverts given move
    /// </summary>
    /// <param name="game">Description of game</param>
    /// <param name="move">Move to revert</param>
    /// <param name="tempSavedTakenFigure">Saved figure in case, that move was capture</param>
    private static void RevertMove(GameData game, GameMove move, Figure tempSavedTakenFigure)
    {
        game.Chessboard.MoveTo(move.Target, move.Source);
        game.Chessboard.AddFigure(tempSavedTakenFigure, move.Target);

        if (move.CastlingRookSource != null && move.CastlingRookTarget != null)
        {
            game.Chessboard.MoveTo(move.CastlingRookTarget, move.CastlingRookSource);
        }
    }

    /// <summary>
    /// Checks whether player has no possible moves to play
    /// </summary>
    /// <param name="game">Description of game</param>
    /// <param name="playerColor">Color of player</param>
    /// <returns>Whether player has no possible moves to play</returns>
    private static bool PlayerHasNoPossibleMoves(GameData game, PlayerColor playerColor)
    {
        var possibleMoves = GetAllMoves(game.Chessboard, playerColor);

        foreach (var move in possibleMoves)
        {
            var tempSavedTakenFigure = game.Chessboard.GetFigureOnPosition(move.Target);
            var isFromMoved = game.Chessboard.GetFigureOnPosition(move.Source).Moved;

            bool isCastlingRookFromMoved = false;
            if (move.CastlingRookSource != null && move.CastlingRookTarget != null)
            {
                isCastlingRookFromMoved = game.Chessboard.GetFigureOnPosition(move.CastlingRookSource).Moved;
            }
            
            PerformMove(game, move);
            bool isChecked = PlayerIsChecked(game, playerColor);
            RevertMove(game, move, tempSavedTakenFigure);
            game.Chessboard.GetFigureOnPosition(move.Source).Moved = isFromMoved;

            if (move.CastlingRookSource != null && move.CastlingRookTarget != null)
            {
                game.Chessboard.GetFigureOnPosition(move.CastlingRookSource).Moved = isCastlingRookFromMoved;
            }

            if (!isChecked)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Transforms player color to win state of player of this color
    /// </summary>
    private static GameState GetWinStateFromPlayerColor(PlayerColor color)
    {
        return color == PlayerColor.White ? GameState.WhiteWin : GameState.BlackWin;
    }

    /// <summary>
    /// Checks wheter given player is checked in given game
    /// </summary>
    /// <param name="game">Description of game</param>
    private static bool PlayerIsChecked(GameData game, PlayerColor color)
    {
        var myKingPosition = GetKingPosition(game.Chessboard.Figures, color);
        var enemyMoves = GetAllMoves(game.Chessboard, GetOppositeColor(color));
        return enemyMoves.Any(x => x.Target.IsEquivalent(myKingPosition));
    }

    /// <summary>
    /// Gets oponents color
    /// </summary>
    private static PlayerColor GetOppositeColor(PlayerColor color)
    {
        return color == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;
    }

    /// <summary>
    /// Gets king position on chessboard for player of given color
    /// </summary>
    private static ChessPosition GetKingPosition(Figure[,] figures, PlayerColor playerColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var figure = figures[x, y];
                if (figure != null && figure.Type == FigureType.King && figure.Color == playerColor)
                {
                    return new ChessPosition(x, y);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Decodes textual representation of move into object
    /// </summary>
    /// <returns>Move if it's valid, null otherwise</returns>
    private static GameMove DecodeMove(GameData game, string move)
    {
        if (move == "e1g1" || move == "e1c1" || move == "e8g8" || move == "e8c8")
        {
            var from = new ChessPosition('e' - 'a', move[1] == '1' ? 0 : 7);
            var to = new ChessPosition((move[2] == 'g' ? 'g' : 'c') - 'a', move[1] == '1' ? 0 : 7);
            var fromFigure = game.Chessboard.GetFigureOnPosition(from);

            if (fromFigure == null || fromFigure.Type != FigureType.King)
                return null;

            // Define rook's initial and target positions based on the move notation
            ChessPosition rookFrom, rookTo;
            if (move[2] == 'g')  // Kingside castling
            {
                rookFrom = new ChessPosition(7, from.Y);
                rookTo = new ChessPosition(5, from.Y);
            }
            else  // Queenside castling
            {
                rookFrom = new ChessPosition(0, from.Y);
                rookTo = new ChessPosition(3, from.Y);
            }

            return new GameMove(from, to, fromFigure.Type, null)
            {
                CastlingRookSource = rookFrom,
                CastlingRookTarget = rookTo,
                CastlingFigure = FigureType.Rook
            };
        }
        else
        {
            var figure = GetFigureForCharacter(move[0]);
            var from = new ChessPosition(GetPositionForCharacter(move[1]), int.Parse(move[2].ToString()) - 1);
            var isCapture = move[3] == 'x';
            var to = new ChessPosition(GetPositionForCharacter(move[4]), int.Parse(move[5].ToString()) - 1);

            var fromFigure = game.Chessboard.GetFigureOnPosition(from);
            var toFigure = game.Chessboard.GetFigureOnPosition(to);

            if (figure != fromFigure.Type)
                return null;
            if (isCapture)
            {
                if (game.Chessboard.GetFigureOnPosition(to) == null)
                    return null;
            }
            else
            {
                if (game.Chessboard.GetFigureOnPosition(to) != null)
                    return null;
            }

            return new GameMove(from, to, fromFigure.Type, toFigure?.Type);
        }
    }

    /// <summary>
    /// Decodes game and tracking state difference into game move object
    /// </summary>
    /// <returns>Move if it's valid, null otherwise</returns>
    private static GameMove DecodeMove(GameData game, TrackingState trackingState)
    {
        var trackingStateFigures = trackingState.Figures;
        var gameFigures = game.Chessboard.GetTrackingStates().Figures;

        var noneToWhite = new List<ChessPosition>();
        var noneToBlack = new List<ChessPosition>();
        var whiteToNone = new List<ChessPosition>();
        var blackToNone = new List<ChessPosition>();
        var whiteToBlack = new List<ChessPosition>();
        var blackToWhite = new List<ChessPosition>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (gameFigures[x, y] == trackingStateFigures[x, y])
                    continue;

                if (gameFigures[x, y] == TrackingFieldState.None && trackingStateFigures[x, y] == TrackingFieldState.White)
                    noneToWhite.Add(new ChessPosition(x, y));

                if (gameFigures[x, y] == TrackingFieldState.None && trackingStateFigures[x, y] == TrackingFieldState.Black)
                    noneToBlack.Add(new ChessPosition(x, y));

                if (gameFigures[x, y] == TrackingFieldState.White && trackingStateFigures[x, y] == TrackingFieldState.None)
                    whiteToNone.Add(new ChessPosition(x, y));

                if (gameFigures[x, y] == TrackingFieldState.Black && trackingStateFigures[x, y] == TrackingFieldState.None)
                    blackToNone.Add(new ChessPosition(x, y));

                if (gameFigures[x, y] == TrackingFieldState.White && trackingStateFigures[x, y] == TrackingFieldState.Black)
                    whiteToBlack.Add(new ChessPosition(x, y));

                if (gameFigures[x, y] == TrackingFieldState.Black && trackingStateFigures[x, y] == TrackingFieldState.White)
                    blackToWhite.Add(new ChessPosition(x, y));
            }
        }

        if (game.PlayerOnMove == PlayerColor.White)
        {
            if (
                whiteToNone.Count == 2 &&
                noneToWhite.Count == 2 &&
                blackToNone.Count == 0 &&
                noneToBlack.Count == 0 &&
                blackToWhite.Count == 0 &&
                whiteToBlack.Count == 0
            )
            {
                if (whiteToNone.Exists(p => p.X == 4 && p.Y == 0) && noneToWhite.Exists(p => p.X == 6 && p.Y == 0) &&
                    whiteToNone.Exists(p => p.X == 7 && p.Y == 0) && noneToWhite.Exists(p => p.X == 5 && p.Y == 0))
                {
                    return new GameMove(
                        new ChessPosition(4, 0), new ChessPosition(6, 0), FigureType.King, null)
                    {
                        CastlingRookSource = new ChessPosition(7, 0),
                        CastlingRookTarget = new ChessPosition(5, 0),
                        CastlingFigure = FigureType.Rook
                    };
                }

                if (whiteToNone.Exists(p => p.X == 4 && p.Y == 0) && noneToWhite.Exists(p => p.X == 2 && p.Y == 0) &&
                    whiteToNone.Exists(p => p.X == 0 && p.Y == 0) && noneToWhite.Exists(p => p.X == 3 && p.Y == 0))
                {
                    return new GameMove(
                        new ChessPosition(4, 0), new ChessPosition(2, 0), FigureType.King, null)
                    {
                        CastlingRookSource = new ChessPosition(0, 0),
                        CastlingRookTarget = new ChessPosition(3, 0),
                        CastlingFigure = FigureType.Rook
                    };
                }
            }

            if (
                whiteToNone.Count == 1 &&
                noneToWhite.Count == 1 &&
                blackToNone.Count == 0 &&
                noneToBlack.Count == 0 &&
                blackToWhite.Count == 0 &&
                whiteToBlack.Count == 0
                )
            {
                return new GameMove(
                    whiteToNone.First(),
                    noneToWhite.First(),
                    game.Chessboard.GetFigureOnPosition(whiteToNone.First()).Type,
                    game.Chessboard.GetFigureOnPosition(noneToWhite.First())?.Type
                    );
            }

            if (
                whiteToNone.Count == 1 &&
                noneToWhite.Count == 0 &&
                blackToNone.Count == 0 &&
                noneToBlack.Count == 0 &&
                blackToWhite.Count == 1 &&
                whiteToBlack.Count == 0
            )
            {
                return new GameMove(
                    whiteToNone.First(),
                    blackToWhite.First(),
                    game.Chessboard.GetFigureOnPosition(whiteToNone.First()).Type,
                    game.Chessboard.GetFigureOnPosition(blackToWhite.First())?.Type
                );
            }
        }
        else
        {
            if (whiteToNone.Count == 0 &&
                noneToWhite.Count == 0 &&
                blackToNone.Count == 2 &&
                noneToBlack.Count == 2 &&
                blackToWhite.Count == 0 &&
                whiteToBlack.Count == 0
            )
            {
                // Kingside Castling (e8 to g8, h8 to f8)
                if (blackToNone.Exists(p => p.X == 4 && p.Y == 7) && noneToBlack.Exists(p => p.X == 6 && p.Y == 7) &&
                    blackToNone.Exists(p => p.X == 7 && p.Y == 7) && noneToBlack.Exists(p => p.X == 5 && p.Y == 7))
                {
                    return new GameMove(
                        new ChessPosition(4, 7), new ChessPosition(6, 7), FigureType.King, null)
                    {
                        CastlingRookSource = new ChessPosition(7, 7),
                        CastlingRookTarget = new ChessPosition(5, 7),
                        CastlingFigure = FigureType.Rook
                    };
                }
                // Queenside Castling (e8 to c8, a8 to d8)
                if (blackToNone.Exists(p => p.X == 4 && p.Y == 7) && noneToBlack.Exists(p => p.X == 2 && p.Y == 7) &&
                    blackToNone.Exists(p => p.X == 0 && p.Y == 7) && noneToBlack.Exists(p => p.X == 3 && p.Y == 7))
                {
                    return new GameMove(
                        new ChessPosition(4, 7), new ChessPosition(2, 7), FigureType.King, null)
                    {
                        CastlingRookSource = new ChessPosition(0, 7),
                        CastlingRookTarget = new ChessPosition(3, 7),
                        CastlingFigure = FigureType.Rook
                    };
                }
            }

            if (
                whiteToNone.Count == 0 &&
                noneToWhite.Count == 0 &&
                blackToNone.Count == 1 &&
                noneToBlack.Count == 1 &&
                blackToWhite.Count == 0 &&
                whiteToBlack.Count == 0
            )
            {
                return new GameMove(
                    blackToNone.First(),
                    noneToBlack.First(),
                    game.Chessboard.GetFigureOnPosition(blackToNone.First()).Type,
                    game.Chessboard.GetFigureOnPosition(noneToBlack.First())?.Type
                );
            }

            if (
                whiteToNone.Count == 0 &&
                noneToWhite.Count == 0 &&
                blackToNone.Count == 1 &&
                noneToBlack.Count == 0 &&
                blackToWhite.Count == 0 &&
                whiteToBlack.Count == 1
            )
            {
                return new GameMove(
                    blackToNone.First(),
                    whiteToBlack.First(),
                    game.Chessboard.GetFigureOnPosition(blackToNone.First()).Type,
                    game.Chessboard.GetFigureOnPosition(whiteToBlack.First())?.Type
                );
            }
        }

        return null;
    }

    /// <summary>
    /// Get all possible moves for given player
    /// </summary>
    private static List<GameMove> GetAllMoves(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();
        acumulator.AddRange(GetMovesForPawns(chessboard, playerColor));
        acumulator.AddRange(GetMovesForKing(chessboard, playerColor));
        acumulator.AddRange(GetMovesForKnights(chessboard, playerColor));
        acumulator.AddRange(GetMovesForRooks(chessboard, playerColor));
        acumulator.AddRange(GetMovesForBishops(chessboard, playerColor));
        acumulator.AddRange(GetMovesForQueens(chessboard, playerColor));
        acumulator.AddRange(GetMovesForCastling(chessboard, playerColor));

        return acumulator;
    }

    /// <summary>
    /// Gets all possible moves of rooks of given player
    /// </summary>
    private static List<GameMove> GetMovesForRooks(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        var up = new GameMoveVector(0, 1);
        var left = new GameMoveVector(-1, 0);
        var down = new GameMoveVector(0, -1);
        var right = new GameMoveVector(1, 0);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var position = new ChessPosition(x, y);
                var figure = chessboard.GetFigureOnPosition(position);
                if (
                    figure != null &&
                    figure.Type == FigureType.Rook &&
                    figure.Color == playerColor
                )
                {
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, up));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, left));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, down));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, right));
                }
            }
        }

        acumulator.RemoveAll(x => x == null);
        return acumulator;
    }

    /// <summary>
    /// Gets all possible moves of bishops of given player
    /// </summary>
    private static List<GameMove> GetMovesForBishops(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        var upLeft = new GameMoveVector(-1, 1);
        var upRight = new GameMoveVector(1, 1);
        var downLeft = new GameMoveVector(-1, -1);
        var downRight = new GameMoveVector(1, -1);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var position = new ChessPosition(x, y);
                var figure = chessboard.GetFigureOnPosition(position);
                if (
                    figure != null &&
                    figure.Type == FigureType.Bishop &&
                    figure.Color == playerColor
                )
                {
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, upLeft));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, upRight));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, downLeft));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, downRight));
                }
            }
        }

        acumulator.RemoveAll(x => x == null);
        return acumulator;
    }

    /// <summary>
    /// Gets all possible moves of queens of given player
    /// </summary>
    private static List<GameMove> GetMovesForQueens(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        var up = new GameMoveVector(0, 1);
        var left = new GameMoveVector(-1, 0);
        var down = new GameMoveVector(0, -1);
        var right = new GameMoveVector(1, 0);
        var upLeft = new GameMoveVector(-1, 1);
        var upRight = new GameMoveVector(1, 1);
        var downLeft = new GameMoveVector(-1, -1);
        var downRight = new GameMoveVector(1, -1);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var position = new ChessPosition(x, y);
                var figure = chessboard.GetFigureOnPosition(position);
                if (
                    figure != null &&
                    figure.Type == FigureType.Queen &&
                    figure.Color == playerColor
                )
                {
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, up));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, left));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, down));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, right));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, upLeft));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, upRight));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, downLeft));
                    acumulator.AddRange(CanMoveOrAttackIterative(chessboard, playerColor, position, downRight));
                }
            }
        }

        acumulator.RemoveAll(x => x == null);
        return acumulator;
    }

    /// <summary>
    /// Gets all possible moves of pawns of given player
    /// </summary>
    private static List<GameMove> GetMovesForPawns(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        var moveVector = playerColor == PlayerColor.White ? new GameMoveVector(0, 1) : new GameMoveVector(0, -1);
        var captureVector1 = playerColor == PlayerColor.White ? new GameMoveVector(1, 1) : new GameMoveVector(1, -1);
        var captureVector2 = playerColor == PlayerColor.White ? new GameMoveVector(-1, 1) : new GameMoveVector(-1, -1);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var position = new ChessPosition(x, y);
                var figure = chessboard.GetFigureOnPosition(position);
                if (
                    figure != null &&
                    figure.Type == FigureType.Pawn &&
                    figure.Color == playerColor
                   )
                {
                    acumulator.Add(CanAttack(chessboard, playerColor, position, position.Add(captureVector1)));
                    acumulator.Add(CanAttack(chessboard, playerColor, position, position.Add(captureVector2)));
                    var moveOnce = CanMove(chessboard, playerColor, position, position.Add(moveVector));
                    acumulator.Add(moveOnce);
                    if (moveOnce != null && !figure.Moved)
                        acumulator.Add(CanMove(chessboard, playerColor, position, moveOnce.Target.Add(moveVector)));
                }
            }
        }

        acumulator.RemoveAll(x => x == null);
        return acumulator;
    }

    /// <summary>
    /// Gets all possible moves of knights of given player
    /// </summary>
    private static List<GameMove> GetMovesForKnights(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        var upLeft = new GameMoveVector(-1, 2);
        var upRight = new GameMoveVector(1, 2);
        var downLeft = new GameMoveVector(-1, -2);
        var downRight = new GameMoveVector(1, -2);
        var leftUp = new GameMoveVector(-2, 1);
        var leftDown = new GameMoveVector(-2, -1);
        var rightUp = new GameMoveVector(2, 1);
        var rightDown = new GameMoveVector(2, -1);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var position = new ChessPosition(x, y);
                var figure = chessboard.GetFigureOnPosition(position);
                if (
                    figure != null &&
                    figure.Type == FigureType.Knight &&
                    figure.Color == playerColor
                )
                {
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(upLeft)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(upRight)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(downLeft)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(downRight)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(leftUp)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(leftDown)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(rightUp)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(rightDown)));
                }
            }
        }

        acumulator.RemoveAll(x => x == null);
        return acumulator;
    }

    private static List<GameMove> GetMovesForCastling(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        // Coordinates for the king's starting position (assumed to be row 0 for White and row 7 for Black)
        int kingRow = playerColor == PlayerColor.White ? 0 : 7;
        int kingColumn = 4; // 'e' file
        var kingPosition = new ChessPosition(kingColumn, kingRow);
        var king = chessboard.GetFigureOnPosition(kingPosition);

        // Check if the king has already moved or if there is no king at the expected position
        if (king == null || king.Type != FigureType.King || king.Moved)
            return acumulator;

        // Check kingside castling (with the h1/h8 rook)
        int kingsideRookColumn = 7; // 'h' file
        var kingsideRookPosition = new ChessPosition(kingsideRookColumn, kingRow);
        var kingsideRook = chessboard.GetFigureOnPosition(kingsideRookPosition);

        if (kingsideRook != null && kingsideRook.Type == FigureType.Rook && !kingsideRook.Moved && IsPathClear(chessboard, kingPosition, kingsideRookPosition))
        {
            acumulator.Add(new GameMove(kingPosition, new ChessPosition(kingColumn + 2, kingRow), king.Type, null)
            {
                CastlingRookSource = kingsideRookPosition,
                CastlingRookTarget = new ChessPosition(kingColumn + 1, kingRow),
                CastlingFigure = FigureType.Rook
            });
        }

        // Check queenside castling (with the a1/a8 rook)
        int queensideRookColumn = 0; // 'a' file
        var queensideRookPosition = new ChessPosition(queensideRookColumn, kingRow);
        var queensideRook = chessboard.GetFigureOnPosition(queensideRookPosition);

        if (queensideRook != null && queensideRook.Type == FigureType.Rook && !queensideRook.Moved && IsPathClear(chessboard, kingPosition, queensideRookPosition))
        {
            acumulator.Add(new GameMove(kingPosition, new ChessPosition(kingColumn - 2, kingRow), king.Type, null)
            {
                CastlingRookSource = queensideRookPosition,
                CastlingRookTarget = new ChessPosition(kingColumn - 1, kingRow),
                CastlingFigure = FigureType.Rook
            });
        }

        return acumulator;
    }

    private static bool IsPathClear(ChessboardModel chessboard, ChessPosition from, ChessPosition to)
    {
        int step = to.X > from.X ? 1 : -1;
        for (int x = from.X + step; x != to.X; x += step)
        {
            if (chessboard.GetFigureOnPosition(new ChessPosition(x, from.Y)) != null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets all possible moves of king of given player
    /// </summary>
    private static List<GameMove> GetMovesForKing(ChessboardModel chessboard, PlayerColor playerColor)
    {
        var acumulator = new List<GameMove>();

        var up = new GameMoveVector(0, 1);
        var left = new GameMoveVector(-1, 0);
        var down = new GameMoveVector(0, -1);
        var right = new GameMoveVector(1, 0);
        var upLeft = new GameMoveVector(-1, 1);
        var upRight = new GameMoveVector(1, 1);
        var downLeft = new GameMoveVector(-1, -1);
        var downRight = new GameMoveVector(1, -1);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var position = new ChessPosition(x, y);
                var figure = chessboard.GetFigureOnPosition(position);
                if (
                    figure != null &&
                    figure.Type == FigureType.King &&
                    figure.Color == playerColor
                )
                {
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(up)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(left)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(down)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(right)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(upLeft)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(upRight)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(downLeft)));
                    acumulator.Add(CanMoveOrAttack(chessboard, playerColor, position, position.Add(downRight)));
                }
            }
        }

        acumulator.RemoveAll(x => x == null);
        return acumulator;
    }

    /// <summary>
    /// Gets moves in direction of vector, until figure of playing player is encountered, or positions ain't valid
    /// </summary>
    /// <param name="playerColor">Capturing player</param>
    /// <param name="from">Position to move from</param>
    /// <param name="vector">Vector to move to</param>
    private static List<GameMove> CanMoveOrAttackIterative(ChessboardModel chessboard, PlayerColor playerColor, ChessPosition from, GameMoveVector vector)
    {
        var acumulator = new List<GameMove>();

        var temp = from;

        while (true)
        {
            temp = temp.Add(vector);

            var attack = CanAttack(chessboard, playerColor, from, temp);
            var move = CanMove(chessboard, playerColor, from, temp);

            acumulator.Add(move);
            acumulator.Add(attack);

            if (attack != null)
                return acumulator;

            if (move == null)
                return acumulator;
        }
    }

    /// <summary>
    /// Gets move, if there is figure of opponent on moving to position, or its empty, null otherwise
    /// </summary>
    /// <param name="playerColor">Moving player</param>
    /// <param name="from">Position to move from</param>
    /// <param name="to">Position to move to</param>
    private static GameMove CanMoveOrAttack(ChessboardModel chessboard, PlayerColor playerColor, ChessPosition from, ChessPosition to)
    {
        if (!to.IsValid())
            return null;

        var moveTo = chessboard.GetFigureOnPosition(to);

        if (moveTo != null && moveTo.Color == playerColor)
            return null;

        return new GameMove(from, to, chessboard.GetFigureOnPosition(from).Type, chessboard.GetFigureOnPosition(to)?.Type);
    }

    /// <summary>
    /// Gets move, if there is figure of opponent on moving to position, null otherwise
    /// </summary>
    /// <param name="playerColor">Capturing player</param>
    /// <param name="from">Position to capture from</param>
    /// <param name="to">Position to capture to</param>
    private static GameMove CanAttack(ChessboardModel chessboard, PlayerColor playerColor, ChessPosition from, ChessPosition to)
    {
        if (!to.IsValid())
            return null;

        var moveTo = chessboard.GetFigureOnPosition(to);

        if (moveTo == null)
            return null;
        if (moveTo.Color == playerColor)
            return null;

        return new GameMove(from, to, chessboard.GetFigureOnPosition(from).Type, chessboard.GetFigureOnPosition(to)?.Type);
    }

    /// <summary>
    /// Gets move, if there is no figure in the place moving to, null otherwise
    /// </summary>
    /// <param name="playerColor">Moving player</param>
    /// <param name="from">Position to move from</param>
    /// <param name="to">Position to move to</param>
    private static GameMove CanMove(ChessboardModel chessboard, PlayerColor playerColor, ChessPosition from, ChessPosition to)
    {
        if (!to.IsValid())
            return null;

        var moveTo = chessboard.GetFigureOnPosition(to);

        if (moveTo != null)
            return null;

        return new GameMove(from, to, chessboard.GetFigureOnPosition(from).Type, chessboard.GetFigureOnPosition(to)?.Type);
    }
}
