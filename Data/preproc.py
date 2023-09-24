import sys
import chess.pgn

pgn = open(sys.argv[1])
game = chess.pgn.read_game(pgn)
board = game.board()

discarded_w = 0
discarded_b = 0

for move in game.mainline_moves():
    if board.piece_at(move.to_square) is not None:
        if board.piece_at(move.to_square).color == chess.WHITE:
            discard_x = discarded_w % 8
            discard_y = -2 - int(discarded_w / 8)
            discarded_w += 1
        else:
            discard_x = discarded_b % 8
            discard_y = 9 + int(discarded_b / 8)
            discarded_b += 1

        print("{} {} {} {} {}".format(
            board.piece_at(move.to_square),
            int(move.to_square / 8),
            move.to_square % 8,
            discard_y,
            discard_x))

    print("{} {} {} {} {}".format(
        board.piece_at(move.from_square),
        int(move.from_square / 8),
        move.from_square % 8,
        int(move.to_square / 8),
        move.to_square % 8))

    # Castling (second move with a rook)
    if board.is_castling(move):
        from_col = 0 if board.is_queenside_castling(move) else 7
        to_col = 3 if board.is_queenside_castling(move) else 5
        from_square = move.from_square - move.from_square % 8 + from_col
        to_square = move.from_square - move.from_square % 8 + to_col
        print("{} {} {} {} {}".format(
            board.piece_at(from_square),
            int(from_square / 8),
            from_square % 8,
            int(to_square / 8),
            to_square % 8))
    board.push(move)
