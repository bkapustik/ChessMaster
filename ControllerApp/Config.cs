using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace ControllerApp
{
    [Serializable]
    public class Config
    {
        public bool Calibrated = false;
        public float CalibrationOriginX = 0f;
        public float CalibrationOriginY = 0f;
        public float CalibrationFarpointX = 0f;
        public float CalibrationFarpointY = 0f;
        public string GameFile = null;
        public int MovesPlayed = 0;
        public bool MovingPiece = false;
        public string Magic = "ChessmasterMagicV1";

        public void CopyChessManagerState(CraneChessManager manager)
        {
            Calibrated = manager.IsCalibrated;
            if (manager.IsCalibrated)
            {
                CalibrationOriginX = manager.Origin.X;
                CalibrationOriginY = manager.Origin.Y;
                CalibrationFarpointX = manager.Farpoint.X;
                CalibrationFarpointY = manager.Farpoint.Y;
            }
        }

        public Vector2? GetCalibrationOrigin()
        {
            if (!Calibrated) return null;
            return new Vector2(CalibrationOriginX, CalibrationOriginY);
        }

        public Vector2? GetCalibrationFarpoint()
        {
            if (!Calibrated) return null;
            return new Vector2(CalibrationFarpointX, CalibrationFarpointY);
        }


        public void SaveAs(string configFile)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(configFile, FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static Config Load(string configFile)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(configFile, FileMode.Open, FileAccess.Read);
            Config config = (Config)formatter.Deserialize(stream);
            if (config.Magic != "ChessmasterMagicV1")
            {
                throw new Exception("Invalid magic value of loaded config file (" + config.Magic + " given but ChessmasterMagicV1 expected).");
            }
            return config;
        }
    }
}
