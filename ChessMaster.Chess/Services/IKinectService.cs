using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;

namespace ChessMaster.ChessDriver.Services;

public interface IKinectService
{
    GameController GameController { get; }
    UserDefinedParametersPrototypeFactory UserDefinedParameters { get; }
}
