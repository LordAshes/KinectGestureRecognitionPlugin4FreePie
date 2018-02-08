#
# Identify Gestures
#
def update(p,g):
	diagnostics.debug("Player %s generated gesture %s" % (p, g))

#
# Identify Processing Of Gestures
#
def process(e):
	diagnostics.debug("%s" % e);

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
  
