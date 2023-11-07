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

        private string MoveInteral(char dimension, float value)
        {
            return $"{dimension}{FloatToString(value)}";
        }

        public string MoveX(float value) => MoveInteral('X', value);
        public string MoveY(float value) => MoveInteral('Y', value);
        public string MoveZ(float value) => MoveInteral('Z', value);
        public string MoveXY(float x, float y) => $"{MoveX(x)} {MoveY(y)}";
        public string Move(float x, float y, float z) => $"{MoveXY(x, y)} {MoveZ(z)}";
        public string MoveHome() => "$H";
        public string OpenGrip() => "M8";
        public string CloseGrip() => "M9";
        public string Reset() => "$X";
        public string State() => "?";
        public string Resume() => "~";
        public string Pause() => "!";
        public string LinearMovement() => "G00";
        public string Info() => "$#";
    }
}
