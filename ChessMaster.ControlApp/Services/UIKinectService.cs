using ChessTracking.Core.ImageProcessing.PipelineData;
using ChessTracking.Core.Services;
using System;

namespace ChessMaster.ControlApp.Services;

public class UIKinectService
{
    private static readonly Lazy<UIKinectService> instance =
        new Lazy<UIKinectService>(() => new UIKinectService());

    public static UIKinectService Instance => instance.Value;

    public GameController GameController { get; private set; }
    public UserDefinedParametersPrototypeFactory UserDefinedParameters { get; set; }

    private UIKinectService()
    {
        UserDefinedParameters = new UserDefinedParametersPrototypeFactory();
        GameController = new GameController();
    }
}
