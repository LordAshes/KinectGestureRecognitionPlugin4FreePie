7.0 KNOWN LIMITATIONS

7.1 Calibration

Most KinectJointRelationship entries use relative relationships between one joint and another and
thus don't require sensor calibration. However, when using Static Reference Points calibration can
become important. Since, currently, the plugin does not support provision of the underlying joint
information (such as joint position) it is not easily possible to create a regular calibration
system.

The work around to this is to create a reverse calibration system where the user position is
calibrated to the Startic Reference Points instead of the other way around. In such a case a Static
Reference point can be set up and then based on the user's relation (LeftOf, RightOf, Before, After)
feedback messages can be generated to tell the user to move Left, Right, Forward and backward in
order to calibrate the user to the reference points.

7.2 Player References

Currently the system does not identify the number of players and/or when players leave the field of
view of the sensor. This means that a user who steps out of view and then returns may be assigned a
new player Id but the plugin user will not know that the old player Id is no longer used (beside the
fact that no gestures will be generated for that player Id).

7.3 Uni-Player Relations

While the plugin supports tracking gestures from multiple users (up to the limit allowed by the
Kinect sensor) the gestures are defined independet of players. This means each player has the
possibility of generating any of the gestures in the gesture list and it is up to the script to
"not react" to the gesture if it is inappropriate for a specific player.

This also means that gestures definition system can't really be used to create gestures that involve
multiple players because there is not provision for specifying which player's joint the gesture
involves.