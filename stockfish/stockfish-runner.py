#
# Scanning for changes in the input file (where whole game is save in UCI notation).
# On change, calls stockfish Chess engine to generate next move (for black player only).
# Encodes the suggested move in output file for Chessmaster robotic arm controller.
#

import os
import time
import chess
import chess.engine
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

SRC_FILE = '../../ChessTracking/ChessTracking/output/currentGame.uci'
OUT_FILE = '../ControllerApp/input/move.cmm'


class ChessmasterEncoder:
    def __init__(self):
        self.discarded_w = 0
        self.discarded_b = 0
        self.board = chess.Board()

    def get_board(self):
        return self.board

    def _encode(self, from_s, to_s):
        return "{} {} {} {} {}".format(self.board.piece_at(from_s), int(from_s / 8), from_s % 8, int(to_s / 8), to_s % 8)

    def encode_move(self, move):
        res = []
        if self.board.piece_at(move.to_square) is not None:
            if self.board.piece_at(move.to_square).color == chess.WHITE:
                discard_x = self.discarded_w % 8
                discard_y = -2 - int(self.discarded_w / 8)
                self.discarded_w += 1
            else:
                discard_x = self.discarded_b % 8
                discard_y = 9 + int(self.discarded_b / 8)
                self.discarded_b += 1

            res.append("{} {} {} {} {}".format(
                self.board.piece_at(move.to_square),
                int(move.to_square / 8),
                move.to_square % 8,
                discard_y,
                discard_x))

        res.append(self._encode(move.from_square, move.to_square))

        # Castling (second move with a rook)
        if self.board.is_castling(move):
            from_col = 0 if self.board.is_queenside_castling(move) else 7
            to_col = 3 if self.board.is_queenside_castling(move) else 5
            from_square = move.from_square - move.from_square % 8 + from_col
            to_square = move.from_square - move.from_square % 8 + to_col
            res.append(self._encode(from_square, to_square))

        self.board.push(move)
        return res


class MyEvent(FileSystemEventHandler):
    def __init__(self, engine):
        self.engine = engine
        self.moves = []

    def make_suggestion(self):
        encoder = ChessmasterEncoder()
        moves = []
        if os.path.exists(SRC_FILE):
            with open(SRC_FILE, "r") as f:
                for line in f:
                    raw_move = line.strip()
                    move = chess.Move.from_uci(raw_move)
                    moves.append(move.uci())
                    encoder.encode_move(move)  # just to update internal state of encoder

            if moves != self.moves and not encoder.get_board().is_game_over() and encoder.get_board().turn == chess.BLACK:
                result = self.engine.play(encoder.get_board(), chess.engine.Limit(time=1))
                print("move #{}: {}".format(len(moves) + 1, result.move.uci()))
                encoded_move = encoder.encode_move(result.move)
                with open('./move.cmm', "w") as f:
                    f.writelines(line + '\n' for line in encoded_move)
                    f.close()
                os.replace('./move.cmm', OUT_FILE)
                self.moves = moves

    def dispatch(self, event):
        if event.event_type == 'modified':
            self.make_suggestion()


if __name__ == "__main__":
    print("Starting...")
    engine = chess.engine.SimpleEngine.popen_uci(r".\stockfish-windows-2022-x86-64-avx2.exe")

    event_handler = MyEvent(engine)
    event_handler.make_suggestion()

    observer = Observer()
    observer.schedule(event_handler, os.path.dirname(SRC_FILE))
    observer.start()
    try:
        while True:
            time.sleep(10)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()
    engine.quit()
