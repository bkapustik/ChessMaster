using ChessMaster.RobotDriver.Driver;
using ChessMaster.RobotDriver.SerialDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.RobotDriver.Robotic;

public class RobotCommand
{
    protected readonly SerialCommandFactory commandFactory;

    public RobotCommand()
    {
        commandFactory = new SerialCommandFactory();
    }

    public virtual SerialCommand GetSerialCommand() => new SerialCommand();
}

public class HomeCommand : RobotCommand
{
    public HomeCommand() : base() { }
    public override SerialCommand GetSerialCommand() => commandFactory.MoveHome();
}

public class MoveCommand : RobotCommand
{
    public float X;
    public float Y;
    public float Z;
    public MoveCommand() : base() { }
    public MoveCommand(float x, float y, float z) : base() 
    {
        X = x;
        Y = y;
        Z = z;
    }
    public MoveCommand(Vector3 vector) : base()
    {
        X = vector.X;
        Y = vector.Y;
        Z = vector.Z;
    }
    public override SerialCommand GetSerialCommand() => commandFactory.Move(X, Y, Z);
}

public class OpenCommand : RobotCommand
{ 
    public OpenCommand() : base() { }
    public override SerialCommand GetSerialCommand() => commandFactory.OpenGrip();  
}

public class CloseCommand : RobotCommand
{ 
    public CloseCommand() : base() { }
    public override SerialCommand GetSerialCommand() => commandFactory.CloseGrip();
}
