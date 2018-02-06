8.0 GETPLAYERINFO() AND GETJOINTINFO()

The following methods can be used to get position information about the player skeletons and their
joints. This allows, for example, calibration of static reference points. The initial player poistion
can be read at startup and the static reference points can be set up dynamically based on the initial
position values.

GetPlayerInfo(playerId = null)

This method returns a OrientationInfo object which has X, Y and Z position properties as well as a
playerId property and a tracked property. The X, Y and Z properties indicate the current position of
the player's skeleton. The playerId returns the playerId of the skeleton. This values matches the
playerId provided unless the provided playerId is null in which case the first tracked playerId
is returned (and the X, Y and Z position are provided for that playerId). The tracked property indciates
if the playerId is being tracked. If the provided playerId was null then the information for the
first tracked player will be returned and thus the tracked property will always be true. However, if
a playerId is provided, the tracked property will indicate if that player is still being tracked.

GetJointInfo(joint, playerId = null)

Similar to the above method except that the X, Y, Z properties are provided for the specified joint
of the specified player. Again if provided playerId is null then the first tracked playerId is used.

Note:

Both these function will return null if gesture recognition has not been started, if there are no
players to track or if there is no corresponding skeleton for the provided playerId (typically
becuase the player is no longer in sensor range). Thus the results of these methods should first
be tested against null. In python scripts this can be done by:

info = GetPlayerInfo();
if not info is None:
   # Do something with the info

There is a know bug that the OrientationInfo class (returned by these methods) does not show up in
the type suggestions. However, it can still be used.