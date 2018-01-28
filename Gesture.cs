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
                JointsActorsUsed.Clear();
                JointsRelativesUsed.Clear();
                JointType? relativeJoint = null;
                foreach (GestureSequences gestureSequence in Gestures)
                {
                    foreach (GestureSequenceStep step in gestureSequence.steps)
                    {
                        foreach (JointRelationship rel in step.SuccessConditions)
                        {
                            if (!JointsActorsUsed.Contains(rel.actor)) { JointsActorsUsed.Add(rel.actor); }
                            relativeJoint = rel.relative.ParseJoint();
                            if (relativeJoint != null) { if (!JointsRelativesUsed.Contains(relativeJoint.Value)) { JointsRelativesUsed.Add(relativeJoint.Value); } }
                        }
                        foreach (JointRelationship rel in step.FailureConditions)
                        {
                            if (!JointsActorsUsed.Contains(rel.actor)) { JointsActorsUsed.Add(rel.actor); }
                            relativeJoint = rel.relative.ParseJoint();
                            if (relativeJoint != null) { if (!JointsRelativesUsed.Contains(relativeJoint.Value)) { JointsRelativesUsed.Add(relativeJoint.Value); } }
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
        public Dictionary<int, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, float>>>> Relationships = new Dictionary<int, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, float>>>>();
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

        private Dictionary<int, Dictionary<JointType, SkeletonPoint>> ReferenceRelationships = new Dictionary<int, Dictionary<JointType, SkeletonPoint>>();

        private List<JointType> JointsActorsUsed = new List<JointType>();
        private List<JointType> JointsRelativesUsed = new List<JointType>();
        private Dictionary<int, Dictionary<string, System.Timers.Timer>> Players = new Dictionary<int, Dictionary<string, System.Timers.Timer>>();

        private Action<GestureResult> _callbackGesture = null;
        private Action<string> _callbackProcessing = null;
        private KinectSensor _sensor = null;

        private void ProcessGestures(Skeleton ActiveSkeleton, Dictionary<JointType, Dictionary<string, Dictionary<Relationship, float>>> Relationships)
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
                                        default:
                                            // Non deviation failure condition has been met. Trigger reset.
                                            stepCondition = false;
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
                    foreach (JointType actor in JointsActorsUsed) { ReferenceRelationships[ActiveSkeleton.TrackingId][actor] = NormalizeJoint(ActiveSkeleton.Joints[actor]); }
                }
                else
                {
                    // Check if conditions for gesture step have been met
                    stepCondition = true;
                    foreach (var rel in gestureStep.SuccessConditions)
                    {
                        if (!Relationships.ContainsKey(rel.actor)) { stepCondition = false; break; }
                        if (!Relationships[rel.actor].ContainsKey(rel.relative)) { stepCondition = false; break; }
                        switch (rel.relation)
                        {
                            case Relationship.XChange:
                            case Relationship.YChange:
                            case Relationship.ZChange:
                                if (rel.deviation < 0)
                                {
                                    if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] > rel.deviation) { stepCondition = false; break; }
                                }
                                else
                                {
                                    if (Relationships[rel.actor][rel.relative.ToString()][rel.relation] < rel.deviation) { stepCondition = false; break; }
                                }
                                break;
                            default:
                                if (!Relationships[rel.actor][rel.relative.ToString()].ContainsKey(rel.relation)) { stepCondition = false; break; }
                                break;
                        }
                    }
                    // If conditions have been met, check if conditions for gesture have been met
                    if (stepCondition)
                    {
                        // Conditions for gesture step have been met
                        if (_callbackProcessing != null) { _callbackProcessing("Player " + ActiveSkeleton.TrackingId + " Has Completed Gesture " + sequence.gesture + " Step " + (sequence.progress + 1) + " Of " + sequence.steps.Count); }
                        foreach (JointType actor in JointsActorsUsed) { ReferenceRelationships[ActiveSkeleton.TrackingId][actor] = NormalizeJoint(ActiveSkeleton.Joints[actor]); }
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
        }

        private void ProcessSkeletons(AllFramesReadyEventArgs e)
        {
            Relationships.Clear();

            using (SkeletonFrame skeletonData = e.OpenSkeletonFrame())
            {
                if (skeletonData == null) { return; }

                Skeletons = new Skeleton[skeletonData.SkeletonArrayLength];
                skeletonData.CopySkeletonDataTo(Skeletons);

                foreach (Skeleton skeleton in Skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.Tracked) { continue; }

                    if (!Relationships.ContainsKey(skeleton.TrackingId)) { Relationships.Add(skeleton.TrackingId, new Dictionary<JointType, Dictionary<string, Dictionary<Relationship, float>>>()); }

                    foreach (JointType actor in JointsActorsUsed)
                    {
                        SkeletonPoint actorPos = NormalizeJoint(skeleton.Joints[actor]);
                        if (!Relationships[skeleton.TrackingId].ContainsKey(actor)) { Relationships[skeleton.TrackingId].Add(actor, new Dictionary<string, Dictionary<Relationship, float>>()); }
                        foreach (JointType relative in JointsRelativesUsed)
                        {
                            SkeletonPoint relativePos = NormalizeJoint(skeleton.Joints[relative]);
                            if (!Relationships[skeleton.TrackingId][actor].ContainsKey(relative.ToString())) { Relationships[skeleton.TrackingId][actor].Add(relative.ToString(), new Dictionary<Relationship, float>()); }
                            if (actorPos.Y < (relativePos.Y * 0.9)) { Relationships[skeleton.TrackingId][actor][relative.ToString()].Add(Relationship.Below, (float)Math.Abs(actorPos.Y - (relativePos.Y * 0.9))); }
                            else if (actorPos.Y > (relativePos.Y * 1.1)) { Relationships[skeleton.TrackingId][actor][relative.ToString()].Add(Relationship.Above, (float)Math.Abs(actorPos.Y - (relativePos.Y * 1.1))); }
                            if (actorPos.X < (relativePos.X * 0.9)) { Relationships[skeleton.TrackingId][actor][relative.ToString()].Add(Relationship.LeftOf, (float)Math.Abs(actorPos.X - (relativePos.X * 0.9))); }
                            else if (actorPos.X > (relativePos.X * 1.1)) { Relationships[skeleton.TrackingId][actor][relative.ToString()].Add(Relationship.RightOf, (float)Math.Abs(actorPos.X - (relativePos.X * 1.1))); }
                            if (actorPos.Z < (relativePos.Z * 0.9)) { Relationships[skeleton.TrackingId][actor][relative.ToString()].Add(Relationship.InfrontOf, (float)Math.Abs(actorPos.Z - (relativePos.Z * 0.9))); }
                            else if (actorPos.Z > (relativePos.Z * 1.1)) { Relationships[skeleton.TrackingId][actor][relative.ToString()].Add(Relationship.Behind, (float)Math.Abs(actorPos.Z - (relativePos.Z * 1.1))); }
                        }
                        foreach (KeyValuePair<string, SkeletonPoint> reference in StaticReferences)
                        {
                            SkeletonPoint relativePos = reference.Value;
                            if (!Relationships[skeleton.TrackingId][actor].ContainsKey(reference.Key)) { Relationships[skeleton.TrackingId][actor].Add(reference.Key, new Dictionary<Relationship, float>()); }
                            if (actorPos.Y < (relativePos.Y * 0.9)) { Relationships[skeleton.TrackingId][actor][reference.Key].Add(Relationship.Above, (float)Math.Abs(actorPos.Y - (relativePos.Y * 0.9))); }
                            else if (actorPos.Y > (relativePos.Y * 1.1)) { Relationships[skeleton.TrackingId][actor][reference.Key].Add(Relationship.Below, (float)Math.Abs(actorPos.Y - (relativePos.Y * 1.1))); }
                            if (actorPos.X < (relativePos.X * 0.9)) { Relationships[skeleton.TrackingId][actor][reference.Key].Add(Relationship.LeftOf, (float)Math.Abs(actorPos.X - (relativePos.X * 0.9))); }
                            else if (actorPos.X > (relativePos.X * 1.1)) { Relationships[skeleton.TrackingId][actor][reference.Key].Add(Relationship.RightOf, (float)Math.Abs(actorPos.X - (relativePos.X * 1.1))); }
                            if (actorPos.Z < (relativePos.Z * 0.9)) { Relationships[skeleton.TrackingId][actor][reference.Key].Add(Relationship.InfrontOf, (float)Math.Abs(actorPos.Z - (relativePos.Z * 0.9))); }
                            else if (actorPos.Z > (relativePos.Z * 1.1)) { Relationships[skeleton.TrackingId][actor][reference.Key].Add(Relationship.Behind, (float)Math.Abs(actorPos.Z - (relativePos.Z * 1.1))); }
                        }
                        if (!ReferenceRelationships.ContainsKey(skeleton.TrackingId)) { ReferenceRelationships.Add(skeleton.TrackingId, new Dictionary<JointType, SkeletonPoint>()); }
                        if (!ReferenceRelationships[skeleton.TrackingId].ContainsKey(actor)) { ReferenceRelationships[skeleton.TrackingId].Add(actor, NormalizeJoint(skeleton.Joints[actor])); }
                        if (!Relationships[skeleton.TrackingId][actor].ContainsKey(actor.ToString())) { Relationships[skeleton.TrackingId][actor].Add(actor.ToString(), new Dictionary<Relationship, float>()); }
                        Relationships[skeleton.TrackingId][actor][actor.ToString()].Add(Relationship.XChange, actorPos.X - ReferenceRelationships[skeleton.TrackingId][actor].X);
                        Relationships[skeleton.TrackingId][actor][actor.ToString()].Add(Relationship.YChange, actorPos.Y - ReferenceRelationships[skeleton.TrackingId][actor].Y);
                        Relationships[skeleton.TrackingId][actor][actor.ToString()].Add(Relationship.ZChange, actorPos.Z - ReferenceRelationships[skeleton.TrackingId][actor].Z);
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

