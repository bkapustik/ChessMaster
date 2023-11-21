using ChessMaster.RobotDriver.SerialDriver;

namespace ChessMaster.RobotDriver.Driver
{
    public class SerialCommandFactory
    {
        private string FloatToString(float value)
        {
            long roundedValue = (long)Math.Round(value * 1000);
            long remainder = Math.Abs(roundedValue) % 1000;
            roundedValue = roundedValue / 1000;
            return $"{roundedValue}.{remainder}";
        }

        private string MoveInternal(char dimension, float value)
        {
            return $"{dimension}{FloatToString(value)}";
        }

        public SerialCommand MoveX(float value) => new(MoveInternal('X', value));
        public SerialCommand MoveY(float value) => new(MoveInternal('Y', value));
        public SerialCommand MoveZ(float value) => new(MoveInternal('Z', value));
        public SerialCommand MoveXY(float x, float y) => new($"{MoveInternal('X', x)} {MoveInternal('Y', y)}");
        public SerialCommand Move(float x, float y, float z) => new($"{MoveInternal('X', x)} {MoveInternal('Y', y)} {MoveInternal('Z', z)}");
        public SerialCommand MoveHome() => new("$H");
        public SerialCommand OpenGrip() => new("M8");
        public SerialCommand CloseGrip() => new("M9");
        public SerialCommand Reset() => new("$X");
        public SerialCommand State() => new("?");
        public SerialCommand Resume() => new("~");
        public SerialCommand Pause() => new("!");
        public SerialCommand LinearMovement() => new("G00");
        public SerialCommand Info() => new("$#");
    }
}
