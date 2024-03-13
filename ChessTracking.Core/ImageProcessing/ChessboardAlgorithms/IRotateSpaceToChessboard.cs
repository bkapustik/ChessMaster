using ChessTracking.Common;

namespace ChessTracking.Core.ImageProcessing.ChessboardAlgorithms;

interface IRotateSpaceToChessboard
{
    void Rotate(Chessboard3DReprezentation boardRepprezentation, CameraSpacePoint[] cspFromdd);
}
