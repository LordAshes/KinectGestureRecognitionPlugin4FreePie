5.0 PLUGIN METHODS

The following methods are available:

GetGesturesFromFile(string xmlFile)

This method loads gestures from an external XML file indicated by the provided parameter. The default
location for files is the FreePie directory but the provided file parameter can include a relative or
absolute location along with the file name if the file is stored elsewhere. Loading gestures from file
replaces any current gesture. As a result, any dynamically added gesture (at runtime) should be added
after this method is used (if this method is to be used).

SetGesturesToFile(string xmlFile)

Writes the current gestures to file. This allows dynamically generated gestures to be save and then
reloaded at a future time using previously discussed method or using the plugin automatic gesture file
load setting.

RecognitionStart()

This starts the recognition process. This should only be called after gestures have been loaded from
file or added dynamically.

RecognitionStop()

This stops the recognition process.

AddGesture(string gestureName)

Adds a new gesture to the Gestures list with the given name. This is the name that is returned when
the gesture is detected.

SetGestureTimeout(long timeout, string gestureName = null)
 
Sets the timeout for the gesture sequence. All steps of a gesture sequence have to occur within the
timeout period in order for the gesture to be recognized. If the gestureName parameter is omitted,
the timeout is applied to the last active gesture (typically the last added gesture).

AddGestureStep(string gestureName = null)

Adds a new gesture step to the gesture sequence. If the gestureName parameter is omitted, the step
is added to the last active gesture (typically the last added gesture).

AddGestureStepSuccessRelationship(KinectJoint actor, KinectJointRelationship relationship,
				  string relative, int deviation,
				  int stepNumber = -1, string gestureName = null)

Adds a new KinectJointRelationship condition to the success conditions of the step. In order for a
step in the sequence to be successful (passed) all of the success condition JointRelationship must
be true. 

The actor parameter is one of the KinectJoint joints. Static Reference Points cannot be used as
as the actor. The relationship is one of the KinectJointRelationship relations. Unlike actor the
relative is a string representation of either one KinectJoint joint or the name of a Static
Reference point. Deviation is used for the ChangeByX, ChangeByY and ChangeByZ relationships, it
should be 0 for other relationships.

If stepNumber is omitted, the last active step is used (typically the last added step).
If gestureName is omitted the last active gesture is used (typically the last added gesture).

AddGestureStepSuccessRelationship(KinectJoint actor, KinectJointRelationship relationship,
				  string relative, int deviation,
				  int stepNumber = -1, string gestureName = null)

Same as AddGestureStepSuccessRelationship(...) above except that it defines a new faiure condition
JointRelationship. If any step failure condition KinectJointRelationship are true then the gesture
recognition fails (goes back to detecting the first step in the sequence).

AddGestureStaticReferencePoint(string referenceName, float x, float y, float z)

Adds a Static Reference Point which can then be used, instead of KinectJoint, as a relative when
defining a KinectJointRelationship. For example, instead of creating a relationship which compares
HandLeft to be LeftOf ShouldLeft one could set up a Static Reference Point a (for example) 0,0,200
with a name of (for example) "Center" and compare HandLeft to be LeftOf Center.

Typically Static Reference Points are used to define areas corresponding to features on the screen
or virtual world (e.g. a virtual button, etc).

SetGestureStepSuccessRelationship(string gestureName, int stepNumber, int conditionNumber, 
				  KinectJoint actor, KinectJointRelationship relationship,
				  string relative, int deviation = 0)

This method is similar to the Add version except this method allows an existing gesture step
KinectJointRelationship to be modified. Unlike the add version gestureName, stepNumber and
conditionNumber are required parameters. Their values can be determined by the return values
of the add methods.

SetGestureStepFailureRelationship(string gestureName, int stepNumber, int conditionNumber, 
				  KinectJoint actor, KinectJointRelationship relationship,
				  string relative, int deviation = 0)

This method is similar to the Add version except this method allows an existing gesture step
KinectJointRelationship to be modified. Unlike the add version gestureName, stepNumber and
conditionNumber are required parameters. Their values can be determined by the return values
of the add methods.

GetRecognizedGesture(bool clear = true)

This method has been removed as of Version 2. Player and gesture information is now returned
as part of the event (see section 4 for details).

GetRecognizedGestureByPlayerId(bool clear = true)

This method has been removed as of Version 2. Player and gesture information is now returned
as part of the event (see section 4 for details).

GetRecognitionProcessEvents(bool clear = true)

This method has been removed as of Version 2. Player and gesture information is now returned
as part of the event (see section 4 for details).

Version()

Returns the version of the Kinect Gesture Recognition Plugin.