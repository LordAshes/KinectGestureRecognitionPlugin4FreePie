using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;

namespace FreePiePlugin
{
    [GlobalEnum]
    public enum KinectJoint
    {
        AnkleLeft = Microsoft.Kinect.JointType.AnkleLeft,
        AnkleRight = Microsoft.Kinect.JointType.AnkleRight,
        ElbowLeft = Microsoft.Kinect.JointType.ElbowLeft,
        ElbowRight = Microsoft.Kinect.JointType.ElbowRight,
        FootLeft = Microsoft.Kinect.JointType.FootLeft,
        FootRight = Microsoft.Kinect.JointType.FootRight,
        HandLeft = Microsoft.Kinect.JointType.HandLeft,
        HandRight = Microsoft.Kinect.JointType.HandRight,
        HipCenter = Microsoft.Kinect.JointType.HipCenter,
        HipLeft = Microsoft.Kinect.JointType.HipLeft,
        HipRight = Microsoft.Kinect.JointType.HipRight,
        KneeLeft = Microsoft.Kinect.JointType.KneeLeft,
        KneeRight = Microsoft.Kinect.JointType.KneeRight,
        ShoulderCenter = Microsoft.Kinect.JointType.ShoulderCenter,
        ShoulderLeft = Microsoft.Kinect.JointType.ShoulderLeft,
        ShoulderRight = Microsoft.Kinect.JointType.ShoulderRight,
        Spine = Microsoft.Kinect.JointType.Spine,
        WristLeft = Microsoft.Kinect.JointType.WristLeft,
        WristRight = Microsoft.Kinect.JointType.WristRight
    }

    [GlobalEnum]
    public enum KinectJointRelationship
    {
        Above = GestureParser.Relationship.Above,
        Behind = GestureParser.Relationship.Behind,
        Below = GestureParser.Relationship.Below,
        InfrontOf = GestureParser.Relationship.InfrontOf,
        LeftOf = GestureParser.Relationship.LeftOf,
        None = GestureParser.Relationship.None,
        RightOf = GestureParser.Relationship.RightOf,
        XChange = GestureParser.Relationship.XChange,
        YChange = GestureParser.Relationship.YChange,
        ZChange = GestureParser.Relationship.ZChange
    }

    [Global(Name = "KinectGestureObjectInfo")]
    public class OrientationInfo
    {
        public int playerId { get; set; }
        public bool tracked { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    [GlobalType(Type = typeof(KinectGesture))]
    public class KinectGesturePlugin : IPlugin
    {
        public KinectGesture global = null;

        public event EventHandler Started = new EventHandler((e, a) => { });

        public Dictionary<int, List<string>> recognizedGestures = new Dictionary<int, List<string>>();
        public List<string> processingEvents = new List<string>();

        public SensorSelector kinectSensor = null;
        public GestureParser gestureProcessor = null;

        private Dictionary<string, object> properties = null;

        private GestureParser.GestureSequences lastGesture;
        private int lastStep = -1;
        private int lastSuccessCondition = -1;
        private int lastFailureCondition = -1;

        public object CreateGlobal()
        {
            this.global = new KinectGesture(this);
            return this.global;
        }

        public Action Start()
        {
            kinectSensor = new SensorSelector((s) =>
            {
                processingEvents.Add("Sensor=" + s.status.ToString());
                this.global.OnUpdateProcess(new EventArgs());
                gestureProcessor = new GestureParser(kinectSensor.Kinect, (e) =>
                {
                    if (!recognizedGestures.ContainsKey(e.Player)) { recognizedGestures.Add(e.Player, new List<string>()); }
                    recognizedGestures[e.Player].Add(e.Gesture);
                    this.global.OnUpdate(new EventArgs());
                }, (e) =>
                {
                    processingEvents.Add(e);
                    this.global.OnUpdateProcess(new EventArgs());
                });
                gestureProcessor.Gestures.Clear();
                if ((bool)this.properties["UseGestureFile"] == true)
                {
                    gestureProcessor.GestureFromFile(this.properties["GestureFileName"].ToString());
                }
                Started(this, new EventArgs());
            },null,(p) =>
            {
                if ((p.key == "Status") && (p.value.ToString() == "NoAvailableSensors")) { throw new Exception("No Kinect Sensor Found"); }
            });
            kinectSensor.GetSensor();
            return null;
        }

        public void Stop()
        {
            if (gestureProcessor != null)
            {
                gestureProcessor.Stop();
                gestureProcessor.Gestures.Clear();
            }
            gestureProcessor = null;
            kinectSensor.ClearSensor();
            kinectSensor = null;
        }

        public string FriendlyName
        {
            get { return "Kinect Gesture Recognition Plugin"; }
        }

        public bool GetProperty(int index, IPluginProperty property)
        {
            switch (index)
            {
                case 0:
                    property.Name = "UseGestureFile";
                    property.Caption = "Use Gesture File";
                    property.HelpText = "Load a gesture file automatically?";
                    property.Choices.Add("Yes", true);
                    property.Choices.Add("No", false);
                    property.DefaultValue = false;
                    return true;
                case 1:
                    property.Name = "GestureFileName";
                    property.Caption = "Gesture File";
                    property.DefaultValue = "Gesture.xml";
                    property.HelpText = "Path And File Name Of Gesture File";
                    return true;
            }
            return false;
        }

        public bool SetProperties(Dictionary<string, object> properties)
        {
            this.properties = properties;
            return true;
        }

        public void DoBeforeNextExecute()
        {
            //This method will be executed each iteration of the script
        }

        public void StartRecognition()
        {
            gestureProcessor.Start();
        }

        public void StopRecognition()
        {
            gestureProcessor.Stop();
        }

        public void AddGesture(string gestureName)
        {
            lastGesture = new GestureParser.GestureSequences() { gesture = gestureName };
            gestureProcessor.Gestures.Add(lastGesture);
        }

        public void SetGestureTimeout(long timeout, string gestureName = null)
        {
            if (gestureName != null) { lastGesture = GetGesture(gestureName); }
            lastGesture.timeout = timeout;
        }

        public int AddGestureStep(string gestureName = null)
        {
            if (gestureName != null) { lastGesture = GetGesture(gestureName); }
            GestureParser.GestureSequenceStep gs = new GestureParser.GestureSequenceStep();
            lastGesture.steps.Add(new GestureParser.GestureSequenceStep());
            lastStep = (lastGesture.steps.Count - 1);
            return lastStep;
        }

        public int AddGestureStepSuccessRelationship(KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation, int stepNumber = -1, string gestureName = null)
        {
            if (gestureName != null) { lastGesture = GetGesture(gestureName); }
            if (stepNumber != -1) { lastStep = stepNumber; }
            GestureParser.JointRelationship jr = new GestureParser.JointRelationship()
            {
                actor = (Microsoft.Kinect.JointType)actor,
                relation = (GestureParser.Relationship)(relationship),
                relative = relative,
                deviation = deviation
            };
            lastGesture.steps.ElementAt(lastStep).SuccessConditions.Add(jr);
            lastSuccessCondition = (lastGesture.steps.ElementAt(lastStep).SuccessConditions.Count - 1);
            return lastSuccessCondition;
        }

        public int AddGestureStepFailureRelationship(KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation, int stepNumber = -1, string gestureName = null)
        {
            if (gestureName != null) { lastGesture = GetGesture(gestureName); }
            if (stepNumber != -1) { lastStep = stepNumber; }
            GestureParser.JointRelationship jr = new GestureParser.JointRelationship()
            {
                actor = (Microsoft.Kinect.JointType)actor,
                relation = (GestureParser.Relationship)relationship,
                relative = relative,
                deviation = deviation
            };
            lastGesture.steps.ElementAt(lastStep).FailureConditions.Add(jr);
            lastFailureCondition = (lastGesture.steps.ElementAt(lastStep).FailureConditions.Count - 1);
            return lastFailureCondition;
        }

        public void AddGestureStaticReferencePoint(string referenceName, float x, float y, float z)
        {
            gestureProcessor.StaticReferences.Add(referenceName, new Microsoft.Kinect.SkeletonPoint() { X = x, Y = y, Z = z });
        }

        public void SetGestureStepSuccessRelationship(string gestureName, int stepNumber, int conditionNumber, KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation = 0)
        {
            lastGesture = GetGesture(gestureName);
            lastStep = stepNumber;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).actor = (Microsoft.Kinect.JointType)actor;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).relation = (GestureParser.Relationship)relationship;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).relative = relative;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).deviation = deviation;
        }

        public void SetGestureStepFailureRelationship(string gestureName, int stepNumber, int conditionNumber, KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation = 0)
        {
            lastGesture = GetGesture(gestureName);
            lastStep = stepNumber;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).actor = (Microsoft.Kinect.JointType)actor;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).relation = (GestureParser.Relationship)relationship;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).relative = relative;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).deviation = deviation;
        }

        public Dictionary<int, List<string>> GetRecognizedGesture(bool clear = true)
        {
            Dictionary<int, List<string>> retList = recognizedGestures;
            if (clear) { recognizedGestures.Clear(); }
            return retList;
        }

        public List<string> GetRecognizedGestureByPlayerId(int playerId, bool clear = true)
        {
            List<string> retList = recognizedGestures[playerId];
            if (clear) { recognizedGestures[playerId].Clear(); }
            return retList;
        }

        public List<string> GetRecognitionProcessEvents(bool clear = true)
        {
            List<string> retList = new List<string>(processingEvents.ToArray());
            if (clear) { processingEvents.Clear(); }
            return retList;
        }

        private GestureParser.GestureSequences GetGesture(string gestureName)
        {
            foreach (GestureParser.GestureSequences gesture in gestureProcessor.Gestures)
            {
                if (gesture.gesture == gestureName) { return gesture; }
            }
            return null;
        }
    }

    [Global(Name = "KinectGestures")]
    public class KinectGesture
    {
        private readonly KinectGesturePlugin thisKinectGesturePlugin;

        public event Action update;
        public void OnUpdate(EventArgs e) { if (update != null) { update(); } }

        public event Action processing;
        public void OnUpdateProcess(EventArgs e) { if (processing != null) { processing(); } }

        public KinectGesture(KinectGesturePlugin plugin)
        {
            this.thisKinectGesturePlugin = plugin;
        }

        public void GetGesturesFromFile(string xmlFile)
        {
            this.thisKinectGesturePlugin.gestureProcessor.GestureFromFile(xmlFile);
        }

        public void SetGesturesToFile(string xmlFile)
        {
            this.thisKinectGesturePlugin.gestureProcessor.GestureToFile(xmlFile);
        }

        public void RecognitionStart()
        {
            this.thisKinectGesturePlugin.StartRecognition();
        }

        public void RecognitionStop()
        {
            this.thisKinectGesturePlugin.StopRecognition();
        }

        public void AddGesture(string gestureName)
        {
            this.thisKinectGesturePlugin.AddGesture(gestureName);
        }

        public void SetGestureTimeout(long timeout, string gestureName = null)
        {
            this.thisKinectGesturePlugin.SetGestureTimeout(timeout, gestureName);
        }

        public int AddGestureStep(string gestureName = null)
        {
            return this.thisKinectGesturePlugin.AddGestureStep(gestureName);
        }

        public int AddGestureStepSuccessRelationship(KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation, int stepNumber = -1, string gestureName = null)
        {
            return this.thisKinectGesturePlugin.AddGestureStepSuccessRelationship(actor,relationship,relative,deviation,stepNumber,gestureName);
        }

        public int AddGestureStepFailureRelationship(KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation, int stepNumber = -1, string gestureName = null)
        {
            return this.thisKinectGesturePlugin.AddGestureStepFailureRelationship(actor, relationship, relative, deviation, stepNumber, gestureName);
        }

        public void AddGestureStaticReferencePoint(string referenceName, float x, float y, float z)
        {
            this.thisKinectGesturePlugin.AddGestureStaticReferencePoint(referenceName,x,y,z);
        }

        public void SetGestureStepSuccessRelationship(string gestureName, int stepNumber, int conditionNumber, KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation = 0)
        {
            this.thisKinectGesturePlugin.SetGestureStepSuccessRelationship(gestureName, stepNumber, conditionNumber, actor, relationship, relative, deviation);
        }

        public void SetGestureStepFailureRelationship(string gestureName, int stepNumber, int conditionNumber, KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation = 0)
        {
            this.thisKinectGesturePlugin.SetGestureStepFailureRelationship(gestureName, stepNumber, conditionNumber, actor, relationship, relative, deviation);
        }

        public Dictionary<int, List<string>> GetRecognizedGesture(bool clear = true)
        {
            return this.thisKinectGesturePlugin.GetRecognizedGesture(clear);
        }

        public List<string> GetRecognizedGestureByPlayerId(int playerId, bool clear = true)
        {
            return this.thisKinectGesturePlugin.GetRecognizedGestureByPlayerId(playerId, clear);
        }
        public List<string> GetRecognitionProcessEvents(bool clear = true)
        {
            return this.thisKinectGesturePlugin.GetRecognitionProcessEvents(clear);
        }

        public OrientationInfo GetPlayerInfo(int? playerId = null)
        {
            if (this.thisKinectGesturePlugin == null) { return null; }
            if (this.thisKinectGesturePlugin.gestureProcessor == null) { return null; }
            if (this.thisKinectGesturePlugin.gestureProcessor.Skeletons == null) { return null; }
            Microsoft.Kinect.Skeleton skeleton = null;
            if (playerId == null)
            {
                foreach (Microsoft.Kinect.Skeleton s in this.thisKinectGesturePlugin.gestureProcessor.Skeletons) { if (s.TrackingState != Microsoft.Kinect.SkeletonTrackingState.NotTracked) { skeleton = s; break; } }
            }
            else
            {
                foreach (Microsoft.Kinect.Skeleton s in this.thisKinectGesturePlugin.gestureProcessor.Skeletons) { if (s.TrackingId == playerId.Value) { skeleton = s; break; } }
            }
            if(skeleton!=null)
            {
                return new OrientationInfo()
                {
                    playerId = skeleton.TrackingId,
                    tracked = (skeleton.TrackingState != Microsoft.Kinect.SkeletonTrackingState.NotTracked),
                    X = skeleton.Position.X,
                    Y = skeleton.Position.Y,
                    Z = skeleton.Position.Z,
                };
            }
            else
            {
                return null;
            }
        }

        public OrientationInfo GetJointInfo(KinectJoint joint, int? playerId = null)
        {
            if (this.thisKinectGesturePlugin == null) { return null; }
            if (this.thisKinectGesturePlugin.gestureProcessor == null) { return null; }
            if (this.thisKinectGesturePlugin.gestureProcessor.Skeletons == null) { return null; }
            Microsoft.Kinect.Skeleton skeleton = null;
            if (playerId == null)
            {
                foreach (Microsoft.Kinect.Skeleton s in this.thisKinectGesturePlugin.gestureProcessor.Skeletons) { if (s.TrackingState != Microsoft.Kinect.SkeletonTrackingState.NotTracked) { skeleton = s; break; } }
            }
            else
            {
                foreach (Microsoft.Kinect.Skeleton s in this.thisKinectGesturePlugin.gestureProcessor.Skeletons) { if (s.TrackingId == playerId.Value) { skeleton = s; break; } }
            }
            if (skeleton != null)
            {
                return new OrientationInfo()
                {
                    playerId = skeleton.TrackingId,
                    tracked = (skeleton.Joints[(Microsoft.Kinect.JointType)joint].TrackingState != Microsoft.Kinect.JointTrackingState.NotTracked),
                    X = skeleton.Joints[(Microsoft.Kinect.JointType)joint].Position.X,
                    Y = skeleton.Joints[(Microsoft.Kinect.JointType)joint].Position.Y,
                    Z = skeleton.Joints[(Microsoft.Kinect.JointType)joint].Position.Z,
                };
            }
            else
            {
                return null;
            }
        }
    }
}
