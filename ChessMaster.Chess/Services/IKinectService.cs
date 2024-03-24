using ChessMaster.ChessDriver.Events;
using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;

namespace ChessMaster.ChessDriver.Services;

public interface IKinectService : IDisposable
{
    GameController GameController { get; }
    UserDefinedParametersPrototypeFactory UserDefinedParameters { get; }
    KinectMoveDetectedEvent? OnKinectMoveDetected { get; set; }
}
