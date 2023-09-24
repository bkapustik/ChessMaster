# How to quickly setup the Chessmaster with chess tracker.

User plays as white, AI as black. 

- Run Chessmaster controller app, calibrate the hand.

- Start external input mode for the controller. The grip hovers 2 lines after last black line (to make room for kinect tracking).

- Run the chess tracker, set up the kinect, make sure it is tracking.

- Run `python stockfish-runner.py` in stockfish dir. It reads output of the tracker
(from a file), uses stockfish engine to compute next move, saves the move to the chessmaster's external input folder.
The input/output fiels can be found in stockfish-runner.py.


# TODO

- Tracker do not know castling!

