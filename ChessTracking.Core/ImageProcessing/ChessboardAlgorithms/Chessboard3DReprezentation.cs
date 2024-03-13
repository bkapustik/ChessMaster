using ChessTracking.Core.Tracking.State;
using ChessTracking.Core.Utils;

namespace ChessTracking.Core.ImageProcessing.ChessboardAlgorithms
{
    public class Chessboard3DReprezentation
    {
        public MyVector3DStruct CornerOfChessboard { get; set; }
        public MyVector3DStruct FieldVector1 { get; set; }
        public MyVector3DStruct FieldVector2 { get; set; }
        public SceneCalibrationSnapshot Snapshot { get; set; }

        public Chessboard3DReprezentation(MyVector3DStruct cornerOfChessboard, MyVector3DStruct fieldVector1, MyVector3DStruct fieldVector2)
        {
            CornerOfChessboard = cornerOfChessboard;
            FieldVector1 = fieldVector1;
            FieldVector2 = fieldVector2;
        }
    }
}
