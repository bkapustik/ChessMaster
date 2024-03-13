using ChessTracking.Core.Tracking.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Core.ImageProcessing.PipelineParts.Events
{
    public delegate void SceneCalibrationSnapshotEvent(object? o, SceneCalibrationSnapshotEventArgs e);

    public class SceneCalibrationSnapshotEventArgs : EventArgs
    { 
        public SceneCalibrationSnapshot Snapshot { get; private set; }

        public SceneCalibrationSnapshotEventArgs(SceneCalibrationSnapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }
}
