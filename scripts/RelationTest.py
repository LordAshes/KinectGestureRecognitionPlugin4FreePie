#
# Identify Gestures
#
def update(p,g):
	diagnostics.debug("Player %s generated %s" % (p, g))

#
# Identify Processing Of Gestures
#
def process(e):
	diagnostics.debug("%s" % e)

#
# Read Joint Position
#
def frame():
	a = KinectGestures.GetRelationshipInfo(KinectJoint.HandLeft, KinectJointRelationship.LeftOf, "ShoulderLeft");
	b = KinectGestures.GetRelationshipInfo(KinectJoint.HandLeft, KinectJointRelationship.RightOf, "ShoulderLeft");
	h = KinectGestures.GetJointInfo(KinectJoint.HandLeft);
	s = KinectGestures.GetJointInfo(KinectJoint.ShoulderLeft);
	diagnostics.debug("%s,%s,%s vs %s,%s,%s" % (h.X,h.Y,h.Z,s.X,s.Y,s.Z))

#
# Setup Gestures
#
if starting:
	#
	# Create New Gesture At Runtime
	#
	# Add a new gesture
	KinectGestures.AddGesture("LeftOf")
	# The gesture must be concluded with 1000ms to be recognized
	KinectGestures.SetGestureTimeout(1000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.LeftOf,"ShoulderLeft",0)
	#
	# Add a new gesture
	KinectGestures.AddGesture("RightOf")
	# The gesture must be concluded with 1000ms to be recognized
	KinectGestures.SetGestureTimeout(1000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	#
	# Add a new gesture
	KinectGestures.AddGesture("Above")
	# The gesture must be concluded with 1000ms to be recognized
	KinectGestures.SetGestureTimeout(1000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Above,"ShoulderLeft",0)
	#
	# Add a new gesture
	KinectGestures.AddGesture("Below")
	# The gesture must be concluded with 1000ms to be recognized
	KinectGestures.SetGestureTimeout(1000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	#
	# Add a new gesture
	KinectGestures.AddGesture("Infront")
	# The gesture must be concluded with 1000ms to be recognized
	KinectGestures.SetGestureTimeout(1000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.InfrontOf,"HandRight",0)
	#
	# Add a new gesture
	KinectGestures.AddGesture("Behind")
	# The gesture must be concluded with 1000ms to be recognized
	KinectGestures.SetGestureTimeout(1000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Behind,"HandRight",0)
	#
	# Subscribe to Gesture Recognition event (lists finished gestures indexed by player ID)
	KinectGestures.update += update
	# Subscribe to Gesture Processing events (lists processing events of partially complete gestures)
	#KinectGestures.processing += process
	# Subscribe to frame
	#KinectGestures.frame += frame
	# Start Gesture Recognition
	KinectGestures.RecognitionStart()

