using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ControllerApp
{
    public enum PieceHeight
    {
        Small,
        Medium,
        High
    }
/*
    struct Field
    {
        public readonly int Col;
        public readonly int Row;

        public Field(int col, int row)
        {
            Col = col;
            Row = row;
        }

        static public bool operator ==(Field a, Field b) => a.Col == b.Col && a.Row == b.Row;
        static public bool operator !=(Field a, Field b) => !(a == b);
    }

    struct Move
    {
        public readonly Field From;
        public readonly Field To;

        public Move(Field from, Field to)
        {
            if (from == to)
            {
                throw new ArgumentException("Move source and destination fileds must differ.");
            }
            From = from;
            To = to;
        }
    }
*/
    public class CraneChessManager
    {
        private ICraneDriver driver;

        public Vector3 Limits { get { return driver.Limits; } }

        // calibration Z-axis (fixed)
        private const float altitudeMoving = 100f;
        private const float altitudeCarying = 170f;
        private static readonly Dictionary<PieceHeight, float> altitudePicking = new Dictionary<PieceHeight, float> {
            { PieceHeight.High, 50f },
            { PieceHeight.Medium, 30f },
            { PieceHeight.Small, 18f },
        };

        // calibration XY (dynamic)
        private bool calibrated = false;
        private Vector2 origin = new Vector2(0f, 0f); // center of a1 field
        private Vector2 farpoint;
        private Vector2 columnDelta;
        private Vector2 rowDelta;
        
        public bool IsCalibrated { get { return calibrated; } }
        public Vector2 Origin { get { return origin; } }
        public Vector2 Farpoint { get { return farpoint; } }

        // internal state
        private bool moving = false;
        private bool paused = false;
        private Vector3 position = new Vector3(0f, 0f, 0f);
        private Vector2 target = new Vector2(0f, 0f);
        private bool carrying = false;
        private PieceHeight pieceHeight = PieceHeight.Small;

        public bool IsMoving { get { return moving; } }
        public bool IsPaused { get { return paused; } }
        public Vector3 Position { get { return position; } }
        public Vector2 PositionXY { get { return new Vector2(position.X, position.Y); } }
        public Vector2 LastTarget { get { return target; } }
        public bool IsCarrying { get { return carrying; } }
        public PieceHeight CarriedPieceHeight { get { return pieceHeight; } }

        public void Calibrate(Vector2 origin, Vector2 farpoint)
        {
            Vector2 diff = (farpoint - origin) / 2.0f;
            if (diff.Length() < 5.0f)
            {
                return; // too short for calibration
            }

            // rotated diffs (clock- and counter clock-wise)
            Vector2 cw = new Vector2(-diff.Y, diff.X);
            Vector2 ccw = new Vector2(diff.Y, -diff.X);

            // compute normalized vectors in column and row directions
            this.origin = origin;
            this.farpoint = farpoint;
            columnDelta = (diff + cw) / 7.0f;
            rowDelta = (diff + ccw) / 7.0f;
            calibrated = true;
        }

        public void InvalidateCalibration()
        {
            calibrated = false;
        }

        /// <summary>
        /// Compute planar coordinates of the center of given field based on current callibration.
        /// </summary>
        /// <param name="col">0..7, 0 = leftmost (col a)</param>
        /// <param name="row">0..7, 0 = base of the white player (designated row 1)</param>
        /// <returns></returns>
        public Vector2 GetCoordinates(int col, int row)
        {
            return origin + (float)col * columnDelta + (float)row * rowDelta;
        }

        public CraneChessManager(ICraneDriver driver)
        {
            this.driver = driver;
        }

        public async Task UpdateState()
        {
            CraneInfo info = await driver.GetState();
            moving = info.State == CraneState.Run;
            paused = info.State == CraneState.Hold;
            position = info.Position;
        }

        public async Task MoveTo(Vector2 dest)
        {
            target = dest;
            await driver.Move(dest.X, dest.Y, carrying ? altitudeCarying : altitudeMoving);
            await UpdateState();
        }

        public async Task MoveToField(int col, int row)
        {
            if (!calibrated)
            {
                throw new Exception("The crane manager is not calibrated!");
            }
            await MoveTo(GetCoordinates(col, row));
        }

        private async Task WaitForStop()
        {
            do
            {
                await Task.Delay(50);
                await UpdateState();
            } while (moving);
        }

        public async Task PickPiece(PieceHeight height)
        {
            if (moving || carrying) return;

            await driver.OpenGrip();
            await driver.MoveZ(altitudePicking[height]);
            await WaitForStop();
            await driver.CloseGrip();
            await Task.Delay(1000);
            await driver.MoveZ(altitudeCarying);
            await WaitForStop();

            carrying = true;
            pieceHeight = height;
            await UpdateState();
        }

        public async Task ReleasePiece()
        {
            if (moving || !carrying) return;

            await driver.MoveZ(altitudePicking[pieceHeight]);
            await WaitForStop();
            await driver.OpenGrip();
            await Task.Delay(1000);
            await driver.MoveZ(altitudeMoving);
            await WaitForStop();
            await driver.CloseGrip();

            carrying = false;
            await UpdateState();
        }

        public async Task Home()
        {
            await driver.Home();
            await Task.Delay(50);
            await UpdateState();
        }

        public async Task Stop()
        {
            await driver.Stop();
            await Task.Delay(50);
            await UpdateState();
        }

        public async Task Pause()
        {
            await driver.Pause();
            await Task.Delay(50);
            await UpdateState();
        }

        public async Task Resume()
        {
            await driver.Resume();
            await Task.Delay(50);
            await UpdateState();
        }
    }
}
