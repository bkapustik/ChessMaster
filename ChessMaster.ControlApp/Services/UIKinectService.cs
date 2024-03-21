using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;

namespace ChessMaster.ControlApp.Services;

public class UIKinectService : IUIKinectService
{
    public GameController GameController { get; private set; }
    public UserDefinedParametersPrototypeFactory UserDefinedParameters { get; set; }

    public UIKinectService()
    {
        UserDefinedParameters = new UserDefinedParametersPrototypeFactory();
        GameController = new GameController();
    }

    public void InitializeTracker()
    {
        GameController.InitializeTracker(UserDefinedParameters);
    }
}
