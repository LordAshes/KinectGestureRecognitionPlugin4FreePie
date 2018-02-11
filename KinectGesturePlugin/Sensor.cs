using System;
using System.Diagnostics;

using Microsoft.Kinect;

namespace FreePiePlugin
{
    /// <summary>
    /// Component that automatically finds a Kinect and handles updates
    /// to the sensor.
    /// </summary>
    public class SensorSelector
    {
        /// <summary>
        /// The sensor that this component has connected to.
        /// </summary>
        public KinectSensor Kinect { get; private set; }

        /// <summary>
        /// The status of the current sensor or why we cannot retrieve a sensor.
        /// </summary>
        public ChooserStatus Status { get; private set; }

        /// <summary>
        /// Local reference to Event Aggregator for publishing events
        /// </summary>
        public Action<KinectSensorChangedEventData> SensorCallback = null;
        public Action<StatusChangedEventArgs> StatusCallback = null;
        public Action<KeyValuePair> PropertyCallback = null;

        /// <summary>
        /// Local reference to mutex object
        /// </summary>
        private readonly object lockObject = new object();

        public class KinectSensorChangedEventData
        {
            public KinectSensor sensor;
            public ChooserStatus status;
        }

        public class KeyValuePair
        {
            public string key;
            public object value;
        }

        public SensorSelector(Action<KinectSensorChangedEventData> sensorCallback, Action<StatusChangedEventArgs> statusCallback = null, Action<KeyValuePair> propertyCallback = null)
        {
            this.SensorCallback = sensorCallback;
            this.StatusCallback = statusCallback;
            this.PropertyCallback = propertyCallback;
        }

        public void GetSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensorsOnStatusChanged;
            GetSensorProcess(null);
        }

        public void GetSensorById(string sensorId)
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensorsOnStatusChanged;
            GetSensorProcess(sensorId);
        }

        public void ClearSensor()
        {
            KinectSensor.KinectSensors.StatusChanged -= KinectSensorsOnStatusChanged;
            PublishGetSensorStatus(null, ChooserStatus.None);
        }

        public void TryResolveConflict()
        {
            lock (lockObject)
            {
                GetSensorProcess(null);
            }
        }

        [Flags]
        public enum ChooserStatus
        {
            /// <summary>
            /// Chooser has not been started or it has been stopped
            /// </summary>
            None = 0x00000000,

            /// <summary>
            /// Don't have a sensor yet, some sensor is initializing, you may not get it
            /// </summary>
            SensorInitializing = 0x00000001,

            /// <summary>
            /// This KinectSensorChooser has a connected and started sensor.
            /// </summary>
            SensorStarted = 0x00000002,

            /// <summary>
            /// There are no sensors available on the system.  If one shows up
            /// we will try to use it automatically.
            /// </summary>
            NoAvailableSensors = 0x00000010,

            /// <summary>
            /// Available sensor is in use by another application
            /// </summary>
            SensorConflict = 0x00000020,

            /// <summary>
            /// The available sensor is not powered.  If it receives power we
            /// will try to use it automatically.
            /// </summary>
            SensorNotPowered = 0x00000040,

            /// <summary>
            /// There is not enough bandwidth on the USB controller available
            /// for this sensor.
            /// </summary>
            SensorInsufficientBandwidth = 0x00000080,

            /// <summary>
            /// Available sensor is not genuine.
            /// </summary>
            SensorNotGenuine = 0x00000100,

            /// <summary>
            /// Available sensor is not supported
            /// </summary>
            SensorNotSupported = 0x00000200,

            /// <summary>
            /// Available sensor has an error
            /// </summary>
            SensorError = 0x00000400,
        }

        private void GetSensorProcess(string requiredConnectionId)
        {
            if (Kinect != null && Kinect.Status == KinectStatus.Connected)
            {
                if (requiredConnectionId == null)
                {
                    // we already have an appropriate sensor
                    Debug.Assert(Status == ChooserStatus.SensorStarted, "Chooser in unexpected state");
                    return;
                }

                if (Kinect.DeviceConnectionId == requiredConnectionId)
                {
                    // we already have the requested sensor
                    Debug.Assert(Status == ChooserStatus.SensorStarted, "Chooser in unexpected state");
                    return;
                }
            }

            KinectSensor newSensor = null;
            ChooserStatus newStatus = 0;

            if (KinectSensor.KinectSensors.Count == 0)
            {
                newStatus = ChooserStatus.NoAvailableSensors;
            }
            else
            {
                foreach (KinectSensor sensor in KinectSensor.KinectSensors)
                {
                    if (requiredConnectionId != null && sensor.DeviceConnectionId != requiredConnectionId)
                    {
                        // client has set a required connection Id and this sensor does not have that Id
                        newStatus |= ChooserStatus.NoAvailableSensors;
                        continue;
                    }

                    if (sensor.Status != KinectStatus.Connected)
                    {
                        // Sensor is in some unusable state
                        newStatus |= GetErrorStatusFromSensor(sensor);
                        continue;
                    }

                    if (sensor.IsRunning)
                    {
                        // Sensor is already in use by this application
                        newStatus |= ChooserStatus.NoAvailableSensors;
                        continue;
                    }

                    // There is a potentially available sensor, try to start it
                    try
                    {
                        sensor.Start();
                    }
                    catch (System.IO.IOException)
                    {
                        // some other app has this sensor.
                        newStatus |= ChooserStatus.SensorConflict;
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        // TODO: In multi-proc scenarios, this is getting thrown at the start before we see IOException.  Need to understand.
                        // some other app has this sensor.
                        newStatus |= ChooserStatus.SensorConflict;
                        continue;
                    }

                    // Woo hoo, we have a started sensor.
                    newStatus = ChooserStatus.SensorStarted;
                    newSensor = sensor;
                    break;
                }
            }
            PublishGetSensorStatus(newSensor, newStatus);
        }

        private void PublishGetSensorStatus(KinectSensor newSensor, ChooserStatus newStatus)
        {
            if (Kinect != newSensor)
            {
                if (Kinect != null) { Kinect.Stop(); Kinect.AudioSource.Stop(); }
                Kinect = newSensor;
                if (this.PropertyCallback != null) { PropertyCallback(new KeyValuePair() { key = "Kinect", value = Kinect }); }
                if (this.SensorCallback != null) { SensorCallback(new KinectSensorChangedEventData() { sensor = Kinect, status = newStatus }); }
            }
            if (Status != newStatus)
            {
                if (this.PropertyCallback != null) { PropertyCallback(new KeyValuePair() { key = "Status", value = newStatus }); }
            }
        }

        private static ChooserStatus GetErrorStatusFromSensor(KinectSensor sensor)
        {
            ChooserStatus retval;
            switch (sensor.Status)
            {
                case KinectStatus.Undefined:
                    retval = ChooserStatus.SensorError;
                    break;
                case KinectStatus.Disconnected:
                    retval = ChooserStatus.SensorError;
                    break;
                case KinectStatus.Connected:
                    // not an error state
                    retval = 0;
                    break;
                case KinectStatus.Initializing:
                    retval = ChooserStatus.SensorInitializing;
                    break;
                case KinectStatus.Error:
                    retval = ChooserStatus.SensorError;
                    break;
                case KinectStatus.NotPowered:
                    retval = ChooserStatus.SensorNotPowered;
                    break;
                case KinectStatus.NotReady:
                    retval = ChooserStatus.SensorError;
                    break;
                case KinectStatus.DeviceNotGenuine:
                    retval = ChooserStatus.SensorNotGenuine;
                    break;
                case KinectStatus.DeviceNotSupported:
                    retval = ChooserStatus.SensorNotSupported;
                    break;
                case KinectStatus.InsufficientBandwidth:
                    retval = ChooserStatus.SensorInsufficientBandwidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sensor");
            }

            return retval;
        }

        private void KinectSensorsOnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (e != null)
            {
                if (this.StatusCallback != null) { this.StatusCallback(e); }
                if ((e.Sensor == this.Kinect) || (this.Kinect == null))
                {
                    GetSensorProcess(null);
                }
            }
        }
    }
}
