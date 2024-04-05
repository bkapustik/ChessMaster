using ChessTracking.Common;
using ChessTracking.Kinect.Mapping;
using MemoryMappedCollections;
using Microsoft.Kinect;
using System;
using System.Diagnostics;

namespace ChessTracking.Kinect
{
    public class Kinect
    {
        public FrameDescription ColorFrameDescription { get; }
        public FrameDescription DepthFrameDescription { get; }
        public FrameDescription InfraredFrameDescription { get; }

        private KinectSensor KinectSensor { get; set; }
        private MultiSourceFrameReader Reader { get; set; }
        private CoordinateMapper CoordinateMapper { get; }

        private SharedMemorySerializedMultiBuffer<KinectData> Buffer { get; }

        public Kinect(SharedMemorySerializedMultiBuffer<KinectData> buffer)
        {
            Buffer = buffer;

            KinectSensor = KinectSensor.GetDefault();

            Reader = KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared);
            Reader.MultiSourceFrameArrived += MultisourceFrameArrived;

            CoordinateMapper = KinectSensor.CoordinateMapper;

            ColorFrameDescription = KinectSensor.ColorFrameSource.FrameDescription;
            DepthFrameDescription = KinectSensor.DepthFrameSource.FrameDescription;
            InfraredFrameDescription = KinectSensor.InfraredFrameSource.FrameDescription;

            KinectSensor.Open();
        }


        /// <summary>
        /// Procedure invoked by Kinect when new data are available
        /// </summary>
        private void MultisourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (KinectSensor == null || Reader == null)
            {
                return;
            }

            // acquire frame data
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            // if the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }

            // declare variables for data from sensor
            ColorFrame colorFrame = null;
            DepthFrame depthFrame = null;
            InfraredFrame infraredFrame = null;

            byte[] colorFrameData = null;
            ushort[] depthData = null;
            ushort[] infraredData = null;
            Microsoft.Kinect.DepthSpacePoint[] pointsFromColorToDepth = null;
            Microsoft.Kinect.ColorSpacePoint[] pointsFromDepthToColor = null;
            Microsoft.Kinect.CameraSpacePoint[] cameraSpacePointsFromDepthData = null;

            try
            {
                // get frames from sensor
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                infraredFrame = multiSourceFrame.InfraredFrameReference.AcquireFrame();

                // If any frame has expired by the time we process this event, return.
                if (colorFrame == null || depthFrame == null || infraredFrame == null)
                {
                    return;
                }

                // use frame data to fill arrays
                colorFrameData = new byte[ColorFrameDescription.LengthInPixels * 4];
                depthData = new ushort[DepthFrameDescription.LengthInPixels];
                infraredData = new ushort[InfraredFrameDescription.LengthInPixels];

                colorFrame.CopyConvertedFrameDataToArray(colorFrameData, ColorImageFormat.Bgra);
                depthFrame.CopyFrameDataToArray(depthData);
                infraredFrame.CopyFrameDataToArray(infraredData);

                pointsFromColorToDepth = new Microsoft.Kinect.DepthSpacePoint[ColorFrameDescription.LengthInPixels];
                pointsFromDepthToColor = new Microsoft.Kinect.ColorSpacePoint[DepthFrameDescription.LengthInPixels];
                cameraSpacePointsFromDepthData = new Microsoft.Kinect.CameraSpacePoint[DepthFrameDescription.LengthInPixels];

                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        pointsFromColorToDepth);

                    CoordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        pointsFromDepthToColor);

                    CoordinateMapper.MapDepthFrameToCameraSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        cameraSpacePointsFromDepthData);
                }
            }
            finally
            {
                // dispose frames so that Kinect can continue processing
                colorFrame?.Dispose();
                depthFrame?.Dispose();
                infraredFrame?.Dispose();

                //TODO zistit ci nejde vypnut nejaky stream - napr. detekcia koncatin

                // send data futher
                if (
                    colorFrameData != null &&
                    depthData != null &&
                    infraredData != null &&
                    cameraSpacePointsFromDepthData != null
                    )
                {
                    var kinectData = new KinectData(
                           colorFrameData,
                           depthData,
                           infraredData,
                           cameraSpacePointsFromDepthData.ToCommon(),
                           pointsFromColorToDepth.ToCommon(),
                           pointsFromDepthToColor.ToCommon());

                    var sw = Stopwatch.StartNew();
                    Buffer.AddOne(ref kinectData);
                    sw.Stop();
                    Debug.WriteLine($"AddOne Took: {sw.ElapsedMilliseconds} ms");
                }
            }
        }

        public void Dispose()
        {
            if (KinectSensor != null)
            {
                KinectSensor.Close();
                KinectSensor = null;
            }

            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
        }
    }
}
