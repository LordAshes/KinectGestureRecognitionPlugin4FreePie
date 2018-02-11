using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Kinect;
using FreePiePlugin.ExtensionMethods;

namespace FreePiePlugin
{
    public class GestureParser
    {
        public GestureParser(KinectSensor sensor, Action<GestureResult> gesturesCallback, Action<string> gestureProcessingCallback = null)
        {
            _sensor = sensor;
            _callbackGesture = gesturesCallback;
            _callbackProcessing = gestureProcessingCallback;
        }

        public void GestureFromFile(string gestureFile)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<GestureSequences>));
            System.IO.StreamReader xmlFile = new System.IO.StreamReader(gestureFile);
            Gestures = (List<GestureSequences>)xml.Deserialize(xmlFile);
            xmlFile.Close();
        }

        public void GestureToFile(string gestureFile)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<GestureSequences>));
            System.IO.StreamWriter xmlFile = new System.IO.StreamWriter(gestureFile);
            xml.Serialize(xmlFile, Gestures);
            xmlFile.Close();
        }

        public void StaticPointsFromFile(string pointsFile)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<StaticPoint>));
            System.IO.StreamReader xmlFile = new System.IO.StreamReader(pointsFile);
            foreach(StaticPoint sp in (List<StaticPoint>)xml.Deserialize(xmlFile)) { StaticReferences.Add(sp.id, sp.position); }
            xmlFile.Close();
        }

        public void StaticPointsToFile(string gestureFile)
        {
            List<StaticPoint> points = new List<StaticPoint>();
            foreach (KeyValuePair<string, SkeletonPoint> sp in StaticReferences) { points.Add(new StaticPoint() { id = sp.Key, position = sp.Value }); }
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<StaticPoint>));
            System.IO.StreamWriter xmlFile = new System.IO.StreamWriter(gestureFile);
            xml.Serialize(xmlFile, points);
            xmlFile.Close();
        }

        public void Start(SkeletonTrackingMode trackingMode = SkeletonTrackingMode.Default)
        {
            if (_sensor != null)
            {
                TrackingMode = trackingMode;
                if (_callbackProcessing != null) { _callbackProcessing("Tracking Mode Set To " + TrackingMode.ToString()); }

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

                _sensor.SkeletonStream.Enable();
                _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                _sensor.AllFramesReady += (s, e) => { ProcessSkeletons(e); };
                _sensor.Start();
            }
        }

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

        public List<GestureSequences> Gestures = new List<GestureSequences>();
        public Dictionary<string, SkeletonPoint> StaticReferences = new Dictionary<string, SkeletonPoint>();
        public Skeleton[] Skeletons = null;
        public Dictionary<int, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>>> Relationships = new Dictionary<int, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>>>();
        public SkeletonTrackingMode TrackingMode { get; private set; } = SkeletonTrackingMode.Default;

        public class GestureResult
        {
            public int Player;
            public string Gesture;
        }

        [Serializable]
        public class StaticPoint
        {
            public string id { get; set; } = "";
            public SkeletonPoint position { get; set; }
        }

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

        [Serializable]
        public class JointRelationship
        {
            public JointType actor { get; set; }
            public Relationship relation { get; set; }
            public string relative { get; set; }
            public int deviation { get; set; } = 0;
        }

        [Serializable]
        public class GestureSequenceStep
        {
            public List<JointRelationship> SuccessConditions { get; set; } = new List<JointRelationship>();
            public List<JointRelationship> FailureConditions { get; set; } = new List<JointRelationship>();
        }

        [Serializable]
        public class GestureSequences
        {
            public string gesture { get; set; } = "";
            public long timeout { get; set; } = 5000;
            public List<GestureSequenceStep> steps { get; set; } = new List<GestureSequenceStep>();
            [System.Xml.Serialization.XmlIgnore]
            public int progress { get; set; } = 0;
        }

        private Dictionary<JointType,Dictionary<string,List<Relationship>>> JointRelationshipsUsed = new Dictionary<JointType, Dictionary<string, List<Relationship>>>();
        private Dictionary<int, Dictionary<JointType, SkeletonPoint>> ReferenceRelationships = new Dictionary<int, Dictionary<JointType, SkeletonPoint>>();

        private Dictionary<int, Dictionary<string, System.Timers.Timer>> Players = new Dictionary<int, Dictionary<string, System.Timers.Timer>>();

        private Action<GestureResult> _callbackGesture = null;
        private Action<string> _callbackProcessing = null;
        private KinectSensor _sensor = null;

        private void ProcessGestures(Skeleton ActiveSkeleton, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, Single>>> Relationships)
        {
            foreach (GestureSequences sequence in Gestures)
            {
                GestureSequenceStep gestureStep = sequence.steps[sequence.progress];
                // Check if conditions for reset have been met
                bool stepCondition = true;
                if (sequence.progress > 0)
                {
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
                if (stepCondition == false)
                {
                    sequence.progress = 0;
                    if (_callbackProcessing != null) { _callbackProcessing("Player " + ActiveSkeleton.TrackingId + " Gesture " + sequence.gesture + " Step Reset"); }
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
                            // Gesture steps complete!
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

        private void ProcessSkeletons(AllFramesReadyEventArgs e)
        {
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
                                        actorPos.X = actorPos.X - relativePos.X; actorPos.Y = actorPos.Y - relativePos.Y; actorPos.Z = actorPos.Z - relativePos.Z;
                                        Relationships[skeleton.TrackingId][actor.Key][relative.Key].Add(relation, Convert.ToSingle(Math.Sqrt((actorPos.X * actorPos.X) + (actorPos.Y * actorPos.Y) + (actorPos.Z * actorPos.Z))));
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

