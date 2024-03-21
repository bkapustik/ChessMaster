using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;

namespace ChessMaster.ControlApp.Services;

public interface IUIKinectService
{
    GameController GameController { get; }
    UserDefinedParametersPrototypeFactory UserDefinedParameters { get; set; }
    void InitializeTracker();
}
