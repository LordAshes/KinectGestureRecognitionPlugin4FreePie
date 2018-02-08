#
# Identify Gestures
#
def update(playerId,gesture):
	diagnostics.debug("UPDATE: Player %s completed gesture %s" % (playerId,gesture))

#
# Identify Processing Of Gestures
#
def process(processingEvent):
	diagnostics.debug("PROCESS: %s" % processingEvent)

#
# Identify Processing Of Gestures
#
def player(playerId,action):
	diagnostics.debug("PLAYER: Player %s %s" % (playerId,action))

#
# Identify Next Frame
#
def frame():
	diagnostics.debug("FRAME: Ready")

#
# Setup Gestures
#
if starting:
	#
	# Create New Gesture At Runtime
	#
	# Add a new gesture called Wave
	KinectGestures.AddGesture("ButtFists")
	# The gesture must be concluded with 5000ms to be recognized
	KinectGestures.SetGestureTimeout(5000)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the first step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Distance,"HandRight",400)
	# Add a new second step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the second step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Distance,"HandRight",-100)
	# Add a new step to the gesture
	step = KinectGestures.AddGestureStep()
	# Add a new success condition to the third step
	KinectGestures.AddGestureStepSuccessRelationship(KinectJoint.HandLeft, KinectJointRelationship.Distance,"HandRight",400)
	# Subscribe to Gesture Recognition event (lists finished gestures indexed by player ID)
	KinectGestures.update += update
	# Subscribe to Gesture Processing events (lists processing events of partially complete gestures)
	KinectGestures.processing += process
	# Subscribe to Player events (lists adding and removal of players)
	KinectGestures.player += player
	# Subscribe to Frame events (indicates when a frame is ready)
	#KinectGestures.frame += frame
	# Start Gesture Recognition
	diagnostics.debug("Using Kinect Gesture Recognition Plugin version %s" % KinectGestures.Version());
	KinectGestures.RecognitionStart()

