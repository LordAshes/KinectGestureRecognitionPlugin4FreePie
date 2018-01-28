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
	# Read Gestures From File (Using Plugin Settings)
	#	
	# Subscribe to Gesture Recognition event (lists finished gestures indexed by player ID)
	KinectGestures.update += update
	# Subscribe to Gesture Processing events (lists processing events of partially complete gestures)
	KinectGestures.processing += process
	# Start Gesture Recognition
	KinectGestures.RecognitionStart()
  