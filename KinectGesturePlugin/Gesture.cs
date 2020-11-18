using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Kinect;
using FreePiePlugin.ExtensionMethods;

namespace FreePiePlugin
{
    /// <summary>
    /// Class for parsing gestures
    /// </summary>
    public class GestureParser
    {
        /// <summary>
        /// Constructor which takes a Kinect sensor and a gesture and processing callback function
        /// </summary>
        /// <param name="sensor">Kinect sensor object which is to be used for gesture recognition</param>
        /// <param name="gesturesCallback">Callback function for recognized gestures</param>
        /// <param name="gestureProcessingCallback">Callback function for partially recognized gestures</param>
        public GestureParser(KinectSensor sensor, Action<GestureResult> gesturesCallback, Action<string> gestureProcessingCallback = null)
        {
            // Sets local references to the provided parameters
            _sensor = sensor;
            _callbackGesture = gesturesCallback;
            _callbackProcessing = gestureProcessingCallback;
        }

        /// <summary>
        /// Function for reading an XML file with a list of gesture sequences and deserializing it into a local List<GestureSequences> object
        /// </summary>
        /// <param name="gestureFile">Drive, path and file name of the file containing the list of gesture sequences</param>
        public void GestureFromFile(string gestureFile)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<GestureSequences>));
            System.IO.StreamReader xmlFile = new System.IO.StreamReader(gestureFile);
            Gestures = (List<GestureSequences>)xml.Deserialize(xmlFile);
            xmlFile.Close();
        }

        /// <summary>
        /// Function for serializing a local List<GestureSequences> object into XML and writing it to a file
        /// </summary>
        /// <param name="gestureFile">Drive, path and file name of the file to which the contents should be written</param>
        public void GestureToFile(string gestureFile)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<GestureSequences>));
            System.IO.StreamWriter xmlFile = new System.IO.StreamWriter(gestureFile);
            xml.Serialize(xmlFile, Gestures);
            xmlFile.Close();
        }

        /// <summary>
        /// Function for reading an XML file with a list of static points and deserializing and adding them to a local dictionary
        /// </summary>
        /// <param name="pointsFile">Drive, path and file name of the file containing the static points</param>
        public void StaticPointsFromFile(string pointsFile)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<StaticPoint>));
            System.IO.StreamReader xmlFile = new System.IO.StreamReader(pointsFile);
            foreach(StaticPoint sp in (List<StaticPoint>)xml.Deserialize(xmlFile)) { StaticReferences.Add(sp.id, sp.position); }
            xmlFile.Close();
        }

        /// <summary>
        /// Function for serializing a local dictionary containing static points into XML and writing it to a file
        /// </summary>
        /// <param name="gestureFile">Drive, path and file name of the file to which the contents should be written</param>
        public void StaticPointsToFile(string gestureFile)
        {
            List<StaticPoint> points = new List<StaticPoint>();
            foreach (KeyValuePair<string, SkeletonPoint> sp in StaticReferences) { points.Add(new StaticPoint() { id = sp.Key, position = sp.Value }); }
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<StaticPoint>));
            System.IO.StreamWriter xmlFile = new System.IO.StreamWriter(gestureFile);
            xml.Serialize(xmlFile, points);
            xmlFile.Close();
        }

        /// <summary>
        /// Function for starting the tracking of the Kinect skeleton in order to recognize gestures
        /// </summary>
        /// <param name="trackingMode">Kinect tracking mode</param>
        public void Start(SkeletonTrackingMode trackingMode = SkeletonTrackingMode.Default)
        {
            // Ensure a sensor has been specified
            if (_sensor != null)
            {
                TrackingMode = trackingMode;
                if (_callbackProcessing != null) { _callbackProcessing("Tracking Mode Set To " + TrackingMode.ToString()); }

                // Build joint relationships based on the provided information
                JointRelationshipsUsed.Clear();
                foreach (GestureSequences gestureSequence in Gestures)
                {
                    foreach (GestureSequenceStep step in gestureSequence.steps)
                    {
                        foreach (JointRelationship rel in step.SuccessConditions)
                        {
                            if (!JointRelationshipsUsed.ContainsKey(rel.actor)) { JointRelationshipsUsed.Add(rel.actor, new Dictionary<string, List<Relationship>>()); }
                            if (!JointRelationshipsUsed[rel.actor].ContainsKey(rel.relative)) { JointRelationshipsUsed[rel.actor].Add(rel.relative, new List<Relationship>()); }
                            if (!JointRelationshipsUsed[rel.actor][rel.relative].Contains(rel.relation)) { JointRelationshipsUsed[rel.actor][rel.relative].Add(rel.relation); }
                        }
                        foreach (JointRelationship rel in step.FailureConditions)
                        {
                            if (!JointRelationshipsUsed.ContainsKey(rel.actor)) { JointRelationshipsUsed.Add(rel.actor, new Dictionary<string, List<Relationship>>()); }
                            if (!JointRelationshipsUsed[rel.actor].ContainsKey(rel.relative)) { JointRelationshipsUsed[rel.actor].Add(rel.relative, new List<Relationship>()); }
                            if (!JointRelationshipsUsed[rel.actor][rel.relative].Contains(rel.relation)) { JointRelationshipsUsed[rel.actor][rel.relative].Add(rel.relation); }
                        }
                    }
                }

                // Enabled Kinect tracking with callback to ProcessSkeleton when each Kinect frame is ready
                _sensor.SkeletonStream.Enable();
                _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _sensor.AllFramesReady += (s, e) => { ProcessSkeletons(e); };
                _sensor.Start();
            }
        }

        /// <summary>
        /// Function for stopping tracking
        /// </summary>
        public void Stop()
        {
            if (_sensor != null)
            {
                if(_sensor.SkeletonStream != null) { _sensor.SkeletonStream.Disable(); }
                _sensor.ColorStream.Disable();
                _sensor.DepthStream.Disable();
                _sensor.AllFramesReady -= (s, e) => { ProcessSkeletons(e); };
                _sensor.Stop();
            }
        }

        // Holds Gesture Sequences (i.e. the individual steps in a gesture for a gesture to be recognized)
        public List<GestureSequences> Gestures = new List<GestureSequences>();

        // Holds Static References (i.e. absolute points as opposed to relative points)
        public Dictionary<string, SkeletonPoint> StaticReferences = new Dictionary<string, SkeletonPoint>();

        // Reference to the Kinect skeletons (Kinect is capable of tracking multiple skeletons)
        public Skeleton[] Skeletons = null;

        // Dictionary for holding relationships between joints
        public Dictionary<int, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>>> Relationships = new Dictionary<int, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>>>();

        // Reference to the tracking mode used 
        public SkeletonTrackingMode TrackingMode { get; private set; } = SkeletonTrackingMode.Default;

        /// <summary>
        /// Class for holding a gesture result inclduing identification of the player and a string indicating the gesture that was recognized
        /// </summary>
        public class GestureResult
        {
            public int Player;
            public string Gesture;
        }

        /// <summary>
        /// class for holding a static point including identification and a Kinect SkeletonPoint
        /// </summary>
        [Serializable]
        public class StaticPoint
        {
            public string id { get; set; } = "";
            public SkeletonPoint position { get; set; }
        }

        /// <summary>
        /// Enumeration of possible joint relationships
        /// </summary>
        [Serializable]
        public enum Relationship
        {
            None = 0,
            Above,
            Below,
            LeftOf,
            RightOf,
            InfrontOf,
            Behind,
            Distance,
            XChange,
            YChange,
            ZChange
        }

        /// <summary>
        /// Class for holding joint relationships
        /// </summary>
        [Serializable]
        public class JointRelationship
        {
            public JointType actor { get; set; }
            public Relationship relation { get; set; }
            public string relative { get; set; }
            public int deviation { get; set; } = 0;
        }

        /// <summary>
        /// Classs for GestureSequenceStep which includes list of successful steps conditions and failure conditions 
        /// </summary>
        [Serializable]
        public class GestureSequenceStep
        {
            public List<JointRelationship> SuccessConditions { get; set; } = new List<JointRelationship>();
            public List<JointRelationship> FailureConditions { get; set; } = new List<JointRelationship>();
        }

        /// <summary>
        /// Class for holding GestureSequences including a list of GestureSequenceStep
        /// </summary>
        [Serializable]
        public class GestureSequences
        {
            public string gesture { get; set; } = "";
            public long timeout { get; set; } = 5000;
            public List<GestureSequenceStep> steps { get; set; } = new List<GestureSequenceStep>();
            [System.Xml.Serialization.XmlIgnore]
            public int progress { get; set; } = 0;
        }

        // Reference for holding used joint relationships. Only used relationships are evaluated during the recognition stage to increase performance (as opposed to evaluating all possible joint relationships)
        private Dictionary<JointType,Dictionary<string,List<Relationship>>> JointRelationshipsUsed = new Dictionary<JointType, Dictionary<string, List<Relationship>>>();

        // Reference between joints and the Kinect skeleton
        private Dictionary<int, Dictionary<JointType, SkeletonPoint>> ReferenceRelationships = new Dictionary<int, Dictionary<JointType, SkeletonPoint>>();

        // Timers for each players by which a gesture sequence must complete in order to be recognized. If the timer expires the gesture sequence is reset.
        private Dictionary<int, Dictionary<string, System.Timers.Timer>> Players = new Dictionary<int, Dictionary<string, System.Timers.Timer>>();

        // Reference to the completed gesture callback function
        private Action<GestureResult> _callbackGesture = null;

        // Reference to the partial gesture completed callback function
        private Action<string> _callbackProcessing = null;

        // Reference to the Kiect sensor being used
        private KinectSensor _sensor = null;

        /// <summary>
        /// Function for processing gesture recognition
        /// </summary>
        /// <param name="ActiveSkeleton">Kinect skeleton being evaluated</param>
        /// <param name="Relationships">Dictionary of relationships to evaluate</param>
        private void ProcessGestures(Skeleton ActiveSkeleton, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>> Relationships)
        {
            foreach (GestureSequences sequence in Gestures)
            {
                GestureSequenceStep gestureStep = sequence.steps[sequence.progress];
                // Check if conditions for reset have been met
                bool stepCondition = true;
                {
                    // Check to see if any failure condition is true
                    foreach (var rel in gestureStep.FailureConditions)
                    {
                        if (Relationships.ContainsKey(rel.actor))
                        {
                            if (Relationships[rel.actor].ContainsKey(rel.relative.ToString()))
                            {
                                if (Relationships[rel.actor][rel.relative.ToString()].ContainsKey(rel.relation))
                                {
                                    switch (rel.relation)
                                    {
                                        case Relationship.XChange:
                                        case Relationship.YChange:
                                        case Relationship.ZChange:
                                            // Check if deviation based failure condition has exceeded the deviation limit
                                            if (rel.deviation < 0)
                                            {
                                                if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] < rel.deviation) { stepCondition = false; break; }
                                            }
                                            else
                                            {
                                                if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] > rel.deviation) { stepCondition = false; break; }
                                            }
                                            break;
                                        case Relationship.Distance:
                                            // Check if distance based failure condition has exceeded the deviation limit
                                            if (rel.deviation < 0)
                                            {
                                                if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] < Math.Abs(rel.deviation)) { stepCondition = false; break; }
                                            }
                                            else
                                            {
                                                if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] > rel.deviation) { stepCondition = false; break; }
                                            }
                                            break;
                                        default:
                                            // Non deviation relationship contain boolean success as the result
                                            if(Relationships[rel.actor][rel.relative.ToString()][rel.relation]==Convert.ToSingle(true))
                                            {
                                                // Non deviation failure condition has been met. Trigger reset.
                                                stepCondition = false;
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                // If any failure conditions have been met, reset the gesture recognition sequence
                if (stepCondition == false)
                {
                    sequence.progress = 0;
                    if (_callbackProcessing != null) { _callbackProcessing("Player " + ActiveSkeleton.TrackingId + " Gesture " + sequence.gesture + " Step Reset"); }
                    if (!ReferenceRelationships.ContainsKey(ActiveSkeleton.TrackingId)) { ReferenceRelationships.Add(ActiveSkeleton.TrackingId, new Dictionary<JointType, SkeletonPoint>()); }
                    foreach (KeyValuePair<JointType, Dictionary<string, List<Relationship>>> actor in JointRelationshipsUsed)
                    {
                        if (!ReferenceRelationships[ActiveSkeleton.TrackingId].ContainsKey(actor.Key))
                        {
                            ReferenceRelationships[ActiveSkeleton.TrackingId].Add(actor.Key, NormalizeJoint(ActiveSkeleton.Joints[actor.Key]));
                        }
                        else
                        {
                            ReferenceRelationships[ActiveSkeleton.TrackingId][actor.Key] = NormalizeJoint(ActiveSkeleton.Joints[actor.Key]);
                        }
                    }
                }
                // Otherwise check if any success conditions have been met
                else
                {
                    // Check if conditions for gesture step have been met
                    stepCondition = true;
                    foreach (var rel in gestureStep.SuccessConditions)
                    {
                        // Console.WriteLine("Distance=" + Relationships[rel.actor][rel.relative.ToString()][Relationship.Distance] + " vs " + rel.deviation);
                        switch (rel.relation)
                        {
                            case Relationship.XChange:
                            case Relationship.YChange:
                            case Relationship.ZChange:
                                // Check if deviation based success condition has exceeded the deviation limit
                                if (rel.deviation < 0)
                                {
                                    if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] > rel.deviation) { stepCondition = false; break; }
                                }
                                else
                                {
                                    if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] < rel.deviation) { stepCondition = false; break; }
                                }
                                break;
                            case Relationship.Distance:
                                // Check if distance based success condition has exceeded the deviation limit
                                if (rel.deviation < 0)
                                {
                                    if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] > Math.Abs(rel.deviation)) { stepCondition = false; break; }
                                }
                                else
                                {
                                    if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] < rel.deviation) { stepCondition = false; break; }
                                }
                                break;
                            default:
                                // Non deviation relationship contain boolean success as the result
                                if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] == Convert.ToSingle(false))
                                {
                                    // Non deviation success condition has not been met
                                    stepCondition = false; break;
                                }
                                break;
                        }
                    }
                    // If conditions have been met, check if conditions for gesture have been met
                    if (stepCondition)
                    {
                        // Conditions for gesture step have been met
                        if (_callbackProcessing != null) { _callbackProcessing("Player " + ActiveSkeleton.TrackingId + " Has Completed Gesture " + sequence.gesture + " Step " + (sequence.progress + 1) + " Of " + sequence.steps.Count); }
                        if (!ReferenceRelationships.ContainsKey(ActiveSkeleton.TrackingId)) { ReferenceRelationships.Add(ActiveSkeleton.TrackingId,new Dictionary<JointType, SkeletonPoint>()); }
                        foreach (KeyValuePair<JointType, Dictionary<string, List<Relationship>>> actor in JointRelationshipsUsed)
                        {
                            if (!ReferenceRelationships[ActiveSkeleton.TrackingId].ContainsKey(actor.Key))
                            {
                                ReferenceRelationships[ActiveSkeleton.TrackingId].Add(actor.Key,NormalizeJoint(ActiveSkeleton.Joints[actor.Key]));
                            }
                            else
                            {
                                ReferenceRelationships[ActiveSkeleton.TrackingId][actor.Key] = NormalizeJoint(ActiveSkeleton.Joints[actor.Key]);
                            }
                        }
                        sequence.progress++;
                        if (sequence.progress >= sequence.steps.Count())
                        {
                            // If all steps in the sequence have been completed then gesture has been recognized
                            if (Players.ContainsKey(ActiveSkeleton.TrackingId)) { if (Players[ActiveSkeleton.TrackingId].ContainsKey(sequence.gesture)) { Players[ActiveSkeleton.TrackingId][sequence.gesture].Enabled = false; } }
                            sequence.progress = 0;
                            if (_callbackProcessing != null) { _callbackProcessing("Player " + ActiveSkeleton.TrackingId + " Has Completed Gesture " + sequence.gesture); }
                            _callbackGesture(new GestureResult() { Player = ActiveSkeleton.TrackingId, Gesture = sequence.gesture });
                        }
                        if (sequence.progress == 1)
                        {
                            if (!Players.ContainsKey(ActiveSkeleton.TrackingId)) { Players.Add(ActiveSkeleton.TrackingId, new Dictionary<string, System.Timers.Timer>()); }
                            if (!Players[ActiveSkeleton.TrackingId].ContainsKey(sequence.gesture))
                            {
                                Players[ActiveSkeleton.TrackingId].Add(sequence.gesture, new System.Timers.Timer());
                                Players[ActiveSkeleton.TrackingId][sequence.gesture].Elapsed += (s, e) =>
                                { if (_callbackProcessing != null) { _callbackProcessing("Player " + ActiveSkeleton.TrackingId + " Gesture " + sequence.gesture + " Reset (Timeout)"); } sequence.progress = 0; Players[ActiveSkeleton.TrackingId][sequence.gesture].Enabled = false; };
                            }
                            Players[ActiveSkeleton.TrackingId][sequence.gesture].Interval = sequence.timeout;
                            Players[ActiveSkeleton.TrackingId][sequence.gesture].Enabled = true;
                        }
                    }
                }
            }
            _callbackProcessing("Frame");
        }

        /// <summary>
        /// Function for processing each skeleton
        /// </summary>
        /// <param name="e">All frames ready event arguments</param>
        private void ProcessSkeletons(AllFramesReadyEventArgs e)
        {
            // Clear current relationship results
            Relationships.Clear();

            using (SkeletonFrame skeletonData = e.OpenSkeletonFrame())
            {
                if (skeletonData == null) { return; }

                Skeletons = new Skeleton[skeletonData.SkeletonArrayLength];
                skeletonData.CopySkeletonDataTo(Skeletons);

                // Cycle Through Each Skeleton
                foreach (Skeleton skeleton in Skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked) { continue; }
                    if (!Relationships.ContainsKey(skeleton.TrackingId)) { Relationships.Add(skeleton.TrackingId, new Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>>()); }

                    // Cycle Through Each Actor
                    foreach (KeyValuePair<JointType, Dictionary<string, List<Relationship>>> actor in JointRelationshipsUsed)
                    {
                        SkeletonPoint actorPos = NormalizeJoint(skeleton.Joints[actor.Key]);
                        if (!Relationships[skeleton.TrackingId].ContainsKey(actor.Key)) { Relationships[skeleton.TrackingId].Add(actor.Key, new Dictionary<string, Dictionary<Relationship, Single>>()); }

                        // Cycle Through Each Relative
                        foreach (KeyValuePair<string, List<Relationship>> relative in actor.Value)
                        {
                            if (!Relationships[skeleton.TrackingId][actor.Key].ContainsKey(relative.Key)) { Relationships[skeleton.TrackingId][actor.Key].Add(relative.Key, new Dictionary<Relationship, Single>()); }

                            // Cycle Through Each Relationship
                            foreach(Relationship relation in relative.Value)
                            {
                                JointType? relativeJoint = relative.Key.ParseJoint();
                                SkeletonPoint relativePos = new SkeletonPoint();
                                if (relativeJoint != null) { relativePos = NormalizeJoint(skeleton.Joints[relativeJoint.Value]); } else { relativePos = StaticReferences[relative.Key]; }
                                switch (relation)
                                {
                                    case Relationship.Above:
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(actorPos.Y >= relativePos.Y));
                                        break;
                                    case Relationship.Below:
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(actorPos.Y < relativePos.Y));
                                        break;
                                    case Relationship.Behind:
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(actorPos.Z >= relativePos.Z));
                                        break;
                                    case Relationship.InfrontOf:
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(actorPos.Z < relativePos.Z));
                                        break;
                                    case Relationship.RightOf:
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(actorPos.X >= relativePos.X));
                                        break;
                                    case Relationship.LeftOf:
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(actorPos.X < relativePos.X));
                                        break;
                                    case Relationship.Distance:
                                        SkeletonPoint deltaPos = new SkeletonPoint(){ X = actorPos.X - relativePos.X, Y = actorPos.Y - relativePos.Y, Z = actorPos.Z - relativePos.Z };
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(Math.Sqrt((deltaPos.X * deltaPos.X) + (deltaPos.Y * deltaPos.Y) + (deltaPos.Z * deltaPos.Z))));
                                        break;
                                    case Relationship.XChange:
                                        if (ReferenceRelationships.ContainsKey(skeleton.TrackingId)) { Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, ReferenceRelationships[skeleton.TrackingId][actor.Key].X - actorPos.X); }
                                        break;
                                    case Relationship.YChange:
                                        float curY = NormalizeJoint(skeleton.Joints[actor.Key]).Y;
                                        if (ReferenceRelationships.ContainsKey(skeleton.TrackingId)) { Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, ReferenceRelationships[skeleton.TrackingId][actor.Key].Y - actorPos.Y); }
                                        break;
                                    case Relationship.ZChange:
                                        float curZ = NormalizeJoint(skeleton.Joints[actor.Key]).Z;
                                        if (ReferenceRelationships.ContainsKey(skeleton.TrackingId)) { Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, ReferenceRelationships[skeleton.TrackingId][actor.Key].Z - actorPos.Z); }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    // Now that the current relationships have been determined process the result for gesture recognition
                    ProcessGestures(skeleton, Relationships[skeleton.TrackingId]);
                }
            }
        }

        public SkeletonPoint NormalizeJoint(Joint joint)
        {
            return new SkeletonPoint() { X = joint.Position.X * 1000, Y = joint.Position.Y * 1000, Z = joint.Position.Z * 1000 };
        }
    }

    namespace ExtensionMethods
    {
        public static class MyExtensions
        {
            public static JointType? ParseJoint(this string name)
            {
                foreach(JointType joint in Enum.GetValues(typeof(JointType)))
                {
                    if (joint.ToString() == name) { return joint; }
                }
                return null;
            }
        }
    }
}

