using ChessTracking.Common;

namespace ChessTracking.Core.ImageProcessing.ChessboardAlgorithms;

interface IRotateSpaceToChessboard
{
    bool TryRotate(Chessboard3DReprezentation boardRepprezentation, CameraSpacePoint[] cspFromdd);
}
