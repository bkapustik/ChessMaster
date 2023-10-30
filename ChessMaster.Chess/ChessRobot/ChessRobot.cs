using ChessMaster.Robot.Driver;
using ChessMaster.Robot.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.ChessDriver.ChessRobot
{
    public class ChessRobot
    {
        private readonly IRobot robot;

        public ChessRobot()
        {
            robot = new Robot.Robot.Robot(new SerialDriver(""));
        }

        public async Task InitializeAsync()
        {
            await robot.Initialize();
        }
    }
}
