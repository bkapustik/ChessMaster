using ChessTracking.Common;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChessTracking.Kinect.Mapping
{
    public static class KinectDataExtensions
    {
        public static ChessTracking.Common.ColorSpacePoint[] ToCommon(this Microsoft.Kinect.ColorSpacePoint[] data)
        {
            var newArray = new Common.ColorSpacePoint[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                newArray[i] = new Common.ColorSpacePoint
                {
                    X = data[i].X,
                    Y = data[i].Y
                };
            }
            return newArray;
        }
        public static ChessTracking.Common.DepthSpacePoint[] ToCommon(this Microsoft.Kinect.DepthSpacePoint[] data)
        {
            var newArray = new Common.DepthSpacePoint[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                newArray[i] = new Common.DepthSpacePoint
                {
                    X = data[i].X,
                    Y = data[i].Y
                };
            }
            return newArray;
        }
        public static ChessTracking.Common.CameraSpacePoint[] ToCommon(this Microsoft.Kinect.CameraSpacePoint[] data)
        {
            var newArray = new Common.CameraSpacePoint[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                newArray[i] = new Common.CameraSpacePoint
                {
                    X = data[i].X,
                    Y = data[i].Y,
                    Z = data[i].Z
                };
            }
            return newArray;
        }
    }
}
