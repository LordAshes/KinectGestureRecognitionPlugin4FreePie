#
# Identify Gestures
#
def update():
	# By default the gesture recognition dictionary is cleared after a GetRecognizedGesture() call so that succesive calls do not include previously identified gestures.
	# To peek at the gesture dictionary without clearing it, call the method with an optional parameter of False.
	gestures = KinectGestures.GetRecognizedGesture()
	for player in gestures:
		for gesture in gestures[player]:
			diagnostics.debug("Player %s generated %s" % (player, gesture))

#
# Identify Processing Of Gestures
#
def process():
	# By default the event processing list is cleared after a GetRecognitionProcessEvents() call so that succesive calls do not include previously identified events.
	# To peek at the event processing list without clearing it, call the method with an optional parameter of False.
	events = KinectGestures.GetRecognitionProcessEvents()
	for event in events:
		diagnostics.debug("%s" % event);

#
# Identify Exceptions
#
def exception():
	diagnostics.debug("%s" % KinectGestures.GetRecognitionProcessEvents().ToString())

#
# Setup Gestures
#
if starting:
	#
	# Create New Gesture At Runtime
	#
	# Add a new gesture called Wave
	KinectGestures.AddGesture("Wave")
	# The gesture must be concluded with 15000ms to be recognized
	KinectGestures.SetGestureTimeout(15000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.LeftOf,"ShoulderLeft",0)
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	# Add a new failure condition to the first step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Add a new second step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the second step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.LeftOf,"ShoulderLeft",0)
	# Add a new success condition to the second step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Above,"ShoulderLeft",0)
	# Add a new failure condition to the second step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Add a new third step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the third step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.LeftOf,"WristLeft",0)
	# Add a new success condition to the third step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Above,"ShoulderLeft",0)
	# Add a new failure condition to the third step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Add a new failure condition to the third step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	# Add a new fourth step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the fourth step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"WristLeft",0)
	# Add a new success condition to the fourth step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Above,"ShoulderLeft",0)
	# Add a new failure condition to the fourth step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Add a new failure condition to the fourth step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	# Add a new fifth step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the fifth step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.LeftOf,"WristLeft",0)
	# Add a new success condition to the fifth step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Above,"ShoulderLeft",0)
	# Add a new failure condition to the fifth step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Add a new failure condition to the fifth step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	# Add a new sixth step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the sixth step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"WristLeft",0)
	# Add a new success condition to the sixth step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Above,"ShoulderLeft",0)
	# Add a new failure condition to the sixth step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Add a new failure condition to the sixth step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	# Add a new seventh to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the seventh step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.LeftOf,"ShoulderLeft",0)
	# Add a new success condition to the seventh step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Below,"ShoulderLeft",0)
	# Add a new failure condition to the seventh step
	KinectGestures.AddGestureStepFailureRelationship(KinectJoint.HandLeft, KinectJointRelationship.RightOf,"ShoulderLeft",0)
	# Subscribe to Gesture Recognition event (lists finished gestures indexed by player ID)
	KinectGestures.update += update
	# Subscribe to Gesture Processing events (lists processing events of partially complete gestures)
	KinectGestures.processing += process
	# Start Gesture Recognition
	KinectGestures.RecognitionStart()
  