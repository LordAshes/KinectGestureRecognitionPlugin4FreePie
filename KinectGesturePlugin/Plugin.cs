using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;

/// <summary>
/// Code for recognizing the resulting library as a FreePie plugin. This allows the FeePie interface to interact with the library.
/// </summary>
namespace FreePiePlugin
{
    // Expose enumerations to FreePie
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
        Head = Microsoft.Kinect.JointType.Head,
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

    // Expose enumerations to FreePie
    [GlobalEnum]
    public enum KinectJointRelationship
    {
        Above = GestureParser.Relationship.Above,
        Behind = GestureParser.Relationship.Behind,
        Below = GestureParser.Relationship.Below,
        Distance = GestureParser.Relationship.Distance,
        InfrontOf = GestureParser.Relationship.InfrontOf,
        LeftOf = GestureParser.Relationship.LeftOf,
        None = GestureParser.Relationship.None,
        RightOf = GestureParser.Relationship.RightOf,
        XChange = GestureParser.Relationship.XChange,
        YChange = GestureParser.Relationship.YChange,
        ZChange = GestureParser.Relationship.ZChange
    }

    // Expose class to FreePie
    [Global(Name = "KinectGestureObjectInfo")]
    public class OrientationInfo
    {
        public int playerId { get; set; }
        public bool tracked { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    // Main plugin class
    [GlobalType(Type = typeof(KinectGesture))]
    public class KinectGesturePlugin : IPlugin
    {
        public KinectGesture global = null;

        public event EventHandler Started = new EventHandler((e, a) => { });

        public SensorSelector kinectSensor = null;
        public GestureParser gestureProcessor = null;

        private Dictionary<string, object> properties = null;

        private GestureParser.GestureSequences lastGesture;
        private int lastStep = -1;
        private int lastSuccessCondition = -1;
        private int lastFailureCondition = -1;

        private List<int> activePlayers = new List<int>();
        private Dictionary<int,int> inactivePlayers = new Dictionary<int, int>();

        public object CreateGlobal()
        {
            this.global = new KinectGesture(this);
            return this.global;
        }

        // Initializes the plugin
        public Action Start()
        {
            kinectSensor = new SensorSelector((s) =>
            {
                this.global.RaiseProcessingEvent("Sensor=" + s.status.ToString());
                gestureProcessor = new GestureParser(kinectSensor.Kinect, (e) =>
                {
                    this.global.RaiseUpdateEvent(e.Player,e.Gesture);
                }, (e) =>
                {
                    if (e.ToUpper() == "FRAME")
                    {
                        this.global.RaiseFrameEvent();
                    }
                    else
                    {
                        this.global.RaiseProcessingEvent(e);
                    }
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

        // Stops the plugin
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

        // Exposed plugin name
        public string FriendlyName
        {
            get { return "Kinect Gesture Recognition Plugin"; }
        }

        // Exposes plugin properties which can be set using the FreePie environment
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
                case 2:
                    property.Name = "DropFrames";
                    property.Caption = "Inactive Frames";
                    property.DefaultValue = "360";
                    property.HelpText = "Remove Player After N Inactive Frames";
                    return true;
            }
            return false;
        }

        // Allows setting of the plugin properties through the FreePie environment
        public bool SetProperties(Dictionary<string, object> properties)
        {
            this.properties = properties;
            return true;
        }

        public void DoBeforeNextExecute()
        {
            if (gestureProcessor != null)
            {
                if (gestureProcessor.Skeletons != null)
                {
                    string ids = "";
                    List<int> currentPlayers = new List<int>();
                    foreach (Microsoft.Kinect.Skeleton s in gestureProcessor.Skeletons)
                    {
                        if (s != null){ ids = ids + s.TrackingId + ","; currentPlayers.Add(s.TrackingId); }
                    }
                    foreach (Microsoft.Kinect.Skeleton s in gestureProcessor.Skeletons)
                    {
                        if (s != null)
                        {
                            if (s.TrackingId != 0)
                            {
                                if (!activePlayers.Contains(s.TrackingId))
                                {
                                    if (inactivePlayers.ContainsKey(s.TrackingId))
                                    {
                                        inactivePlayers.Remove(s.TrackingId);
                                        this.global.RaisePlayerEvent(s.TrackingId, "Active");
                                    }
                                    else
                                    {
                                        this.global.RaisePlayerEvent(s.TrackingId,"Added");
                                    }
                                }
                            }
                            ids = ids + s.TrackingId + ",";
                        }
                    }
                    foreach (int id in activePlayers)
                    {
                        if (id != 0)
                        {
                            if (!currentPlayers.Contains(id))
                            {
                                inactivePlayers.Add(id, 1);
                                this.global.RaisePlayerEvent(id,"Inactivated For 1 Of "+this.properties["DropFrames"]+" Frames");
                            }
                        }
                    }
                    activePlayers = currentPlayers;
                    for(int p = 0; p<inactivePlayers.Count; p++)
                    {
                        int id = inactivePlayers.ElementAt(p).Key;
                        inactivePlayers[id]++;
                        this.global.RaisePlayerEvent(id,"Inactive For "+ inactivePlayers[id] + " Of " + this.properties["DropFrames"] + " Frames");
                        if (inactivePlayers[id] >= int.Parse(this.properties["DropFrames"].ToString()))
                        {
                            inactivePlayers.Remove(id);
                            p--;
                            this.global.RaisePlayerEvent(id,"Removed");
                        }
                    }
                }
            }
        }

        // Exposes the library Start function
        public void StartRecognition()
        {
            gestureProcessor.Start();
        }

        // Exposes the library Stop function
        public void StopRecognition()
        {
            gestureProcessor.Stop();
        }

        // Exposes the library's ability to add a gesture
        public void AddGesture(string gestureName)
        {
            lastGesture = new GestureParser.GestureSequences() { gesture = gestureName };
            gestureProcessor.Gestures.Add(lastGesture);
        }

        // Exposes the library's ability to set the gesture timeout
        public void SetGestureTimeout(long timeout, string gestureName = null)
        {
            if (gestureName != null) { lastGesture = GetGesture(gestureName); }
            lastGesture.timeout = timeout;
        }

        // Exposes the library's ability to add a gesture step
        public int AddGestureStep(string gestureName = null)
        {
            if (gestureName != null) { lastGesture = GetGesture(gestureName); }
            GestureParser.GestureSequenceStep gs = new GestureParser.GestureSequenceStep();
            lastGesture.steps.Add(new GestureParser.GestureSequenceStep());
            lastStep = (lastGesture.steps.Count - 1);
            return lastStep;
        }

        // Exposes the library's ability to add a gesture step's success condition
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

        // Exposes the library's ability to add a gesture step's failure condition
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

        // Exposes the library's ability to add a static reference point
        public void AddGestureStaticReferencePoint(string referenceName, float x, float y, float z)
        {
            gestureProcessor.StaticReferences.Add(referenceName, new Microsoft.Kinect.SkeletonPoint() { X = x, Y = y, Z = z });
        }

        // Exposes the library's ability to set a gesture step's success relationship
        public void SetGestureStepSuccessRelationship(string gestureName, int stepNumber, int conditionNumber, KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation = 0)
        {
            lastGesture = GetGesture(gestureName);
            lastStep = stepNumber;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).actor = (Microsoft.Kinect.JointType)actor;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).relation = (GestureParser.Relationship)relationship;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).relative = relative;
            lastGesture.steps.ElementAt(stepNumber).SuccessConditions.ElementAt(conditionNumber).deviation = deviation;
        }

        // Exposes the library's ability to set a gesture step's failure relationship
        public void SetGestureStepFailureRelationship(string gestureName, int stepNumber, int conditionNumber, KinectJoint actor, KinectJointRelationship relationship, string relative, int deviation = 0)
        {
            lastGesture = GetGesture(gestureName);
            lastStep = stepNumber;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).actor = (Microsoft.Kinect.JointType)actor;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).relation = (GestureParser.Relationship)relationship;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).relative = relative;
            lastGesture.steps.ElementAt(stepNumber).FailureConditions.ElementAt(conditionNumber).deviation = deviation;
        }

        // Exposes the library's ability to get a gesture
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

        public event UpdateHandler update;
        public delegate void UpdateHandler(int playerId, string gesture);
        public void RaiseUpdateEvent(int playerId, string gesture) { if (update != null) { update(playerId, gesture); } }

        public event ProcessingHandler processing;
        public delegate void ProcessingHandler(string processingEvent);
        public void RaiseProcessingEvent(string processingEvent) { if (processing != null) { processing(processingEvent); } }

        public event PlayerHandler player;
        public delegate void PlayerHandler(int playerId, string action);
        public void RaisePlayerEvent(int playerId, string action) { if (player != null) { player(playerId, action); } }

        public event FrameHandler frame;
        public delegate void FrameHandler();
        public void RaiseFrameEvent() { if (frame != null) { frame(); } }

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
                Microsoft.Kinect.SkeletonPoint jointPos = this.thisKinectGesturePlugin.gestureProcessor.NormalizeJoint(skeleton.Joints[(Microsoft.Kinect.JointType)joint]);
                return new OrientationInfo()
                {
                    playerId = skeleton.TrackingId,
                    tracked = (skeleton.Joints[(Microsoft.Kinect.JointType)joint].TrackingState != Microsoft.Kinect.JointTrackingState.NotTracked),
                    X = jointPos.X,
                    Y = jointPos.Y,
                    Z = jointPos.Z,
                };
            }
            else
            {
                return null;
            }
        }

        public float GetRelationshipInfo(KinectJoint actor, KinectJointRelationship relation, string relative)
        {
            if (this.thisKinectGesturePlugin == null) { return float.NaN; }
            if (this.thisKinectGesturePlugin.gestureProcessor == null) { return float.NaN; }
            if (this.thisKinectGesturePlugin.gestureProcessor.Skeletons == null) { return float.NaN; }
            Microsoft.Kinect.Skeleton skeleton = null;
            foreach (Microsoft.Kinect.Skeleton s in this.thisKinectGesturePlugin.gestureProcessor.Skeletons) { if (s.TrackingState != Microsoft.Kinect.SkeletonTrackingState.NotTracked) { skeleton = s; break; } }
            if (skeleton != null)
            {
                if (this.thisKinectGesturePlugin.gestureProcessor.Relationships.ContainsKey(skeleton.TrackingId))
                {
                    if (this.thisKinectGesturePlugin.gestureProcessor.Relationships[skeleton.TrackingId].ContainsKey((Microsoft.Kinect.JointType)actor))
                    {
                        if (this.thisKinectGesturePlugin.gestureProcessor.Relationships[skeleton.TrackingId][(Microsoft.Kinect.JointType)actor].ContainsKey(relative))
                        {
                            if(this.thisKinectGesturePlugin.gestureProcessor.Relationships[skeleton.TrackingId][(Microsoft.Kinect.JointType)actor][relative].ContainsKey((GestureParser.Relationship)relation))
                            {
                                return this.thisKinectGesturePlugin.gestureProcessor.Relationships[skeleton.TrackingId][(Microsoft.Kinect.JointType)actor][relative][(GestureParser.Relationship)relation];
                            }
                            return float.NaN;
                        }
                        return float.NaN;
                    }
                    return float.NaN;
                }
                return float.NaN;
            }
            return float.NaN;
        }

        public string Version()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
