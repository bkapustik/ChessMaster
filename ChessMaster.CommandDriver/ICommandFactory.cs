using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessMaster.CommandFactory
{
    public interface ICommandFactory
    {
        public string MoveX(float value);
        public string MoveY(float value);
        public string MoveZ(float value);
        public string MoveXY(float x, float y);
        public string Move(float x, float y, float z);
        public string MoveHome();
        public string OpenGrip();
        public string CloseGrip();
        public string Reset();
        public string State();
        public string Pause();
        public string Resume();
        public string Info();
        public string LinearMovement();
    }
}
