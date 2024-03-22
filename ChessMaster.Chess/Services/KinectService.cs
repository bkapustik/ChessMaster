using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;

namespace ChessMaster.ChessDriver.Services;

public class KinectService : IKinectService
{
    public GameController GameController { get; private set; }
    public UserDefinedParametersPrototypeFactory UserDefinedParameters { get; set; }

    public KinectService()
    {
        UserDefinedParameters = new UserDefinedParametersPrototypeFactory();
        GameController = new GameController(UserDefinedParameters);
    }
}
