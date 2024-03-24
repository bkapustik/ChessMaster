using ChessTracking.Common;
using ChessTracking.Core.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ChessTracking.Core.ImageProcessing.ChessboardAlgorithms;

public class RotateSpaceToChessboard : IRotateSpaceToChessboard
{
    public bool TryRotate(Chessboard3DReprezentation boardRepresentation, CameraSpacePoint[] csp)
    {
        var firstVector = boardRepresentation.FieldVector1;
        var secondVector = boardRepresentation.FieldVector2;
        var cornerPoint = boardRepresentation.CornerOfChessboard;

        firstVector = MyVector3DStruct.Normalize(firstVector);
        secondVector = MyVector3DStruct.Normalize(secondVector);

        var a = MyVector3DStruct.CrossProduct(ref firstVector, ref secondVector);
        var b = MyVector3DStruct.CrossProduct(ref secondVector, ref firstVector);
        var xVec = new MyVector3DStruct();
        var yVec = new MyVector3DStruct();
        var zVec = new MyVector3DStruct();

        // get new base based on two known vectors and their cross product
        if ((a.z > 0 && b.z < 0)) // originally (a.z > 0 && b.z < 0)
        {
            zVec = a; // a
            yVec = MyVector3DStruct.Normalize(MyVector3DStruct.CrossProduct(ref xVec, ref zVec));

        }
        else if ((a.z < 0 && b.z > 0)) // originally (a.z < 0 && b.z > 0)
        {
            zVec = b; // b
            yVec = MyVector3DStruct.Normalize(MyVector3DStruct.CrossProduct(ref xVec, ref zVec));
        }
        else
        {
            throw new InvalidOperationException();
        }

        // normalize base
        xVec = MyVector3DStruct.Normalize(firstVector);
        yVec = MyVector3DStruct.Normalize(secondVector);
        zVec = MyVector3DStruct.Normalize(zVec);

        // get inverse mapping to base
        double[,] matrixArray =
        {
            {xVec.x, yVec.x, zVec.x},
            {xVec.y, yVec.y, zVec.y},
            {xVec.z, yVec.z, zVec.z}
        };

        Matrix<double> matrix = DenseMatrix.OfArray(matrixArray);

        // Inverting the matrix
        Matrix<double> inverseMatrix = matrix.Inverse();

        // If needed, convert the inverse matrix back to a 2D array or use it directly for further operations
        double[,] inverseArray = inverseMatrix.ToArray();

        // move all points
        for (int i = 0; i < csp.Length; i++)
        {
            // translation to corner point
            var nx = (float)(csp[i].X - cornerPoint.x);
            var ny = (float)(csp[i].Y - cornerPoint.y);
            var nz = (float)(csp[i].Z - cornerPoint.z);

            // rotation around it to given base
            csp[i].X = (float)(inverseArray[0, 0] * nx + inverseArray[0, 1] * ny + inverseArray[0, 2] * nz);
            csp[i].Y = (float)(inverseArray[1, 0] * nx + inverseArray[1, 1] * ny + inverseArray[1, 2] * nz);
            csp[i].Z = (float)(inverseArray[2, 0] * nx + inverseArray[2, 1] * ny + inverseArray[2, 2] * nz);
        }

        return true;
    }
}
