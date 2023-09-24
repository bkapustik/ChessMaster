import sys
import chess
import chess.engine

engine = chess.engine.SimpleEngine.popen_uci(r".\stockfish-windows-2022-x86-64-avx2.exe")

print("Starting...")
board = chess.Board()
while not board.is_game_over():
    raw_move = sys.stdin.readline().strip()
    move = chess.Move.from_uci(raw_move)
    board.push(move)

    result = engine.play(board, chess.engine.Limit(time=1))
    print(result.move.uci())
    board.push(result.move)

engine.quit()
