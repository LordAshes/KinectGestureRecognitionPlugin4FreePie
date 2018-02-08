B.0 VERSION 2

B.1 Optimization Overhaul

Version 2 of the Kinect Gesture Recognition Plugin consisted primarily of an overhaul of the gesture
recognition code to optimize the recognition process. Prior to Version 2, the plugin identified all
actors and relatives used in any of the configured gestures and each frame evaluated all relationships
for each actor with each relative. This ment that many relationship that were not being used were
calculated because of actor-relative combinations that we not use in any gestures or relationships
that were not used. Version 2 of the plugin only evaluates relationships which are being used in some
configured gesture for specific actor-relative combinations.

For example, if one gesture uses LeftHand is LeftOf LeftShoulder and a second gesture uses RightFoot
is RightOf Spine then Version 1 of the plugin would have evaluated all relationships between
LeftHand and LeftShoulder, LeftHand and Spine, RightFoot and LeftShoulder and RightFoot and Spine.
Version 2 of the plugin, on the other hand, would just evalue the LeftHand is LeftOf LeftShoulder
relationship and the RightFoot is RightOf Spine relationship.

B.2 Direct Event Infromation

The other significant change to the plugin in Version 2, as per the updated previous documentation,
is that there are 4 events which can be subscribed to (update, processing, player, and frame) and
these events provide their corresponding information directly. As a result the GetRecognizedGesture,
GetRecognizedGestureByPlayerId, GetRecognitionProcessEvents, and GetRecognitionChangeEvents have
been removed. Instead the infromation is provided directly through the events:

update([int]playerId, [string]gesture)
processing([string]processingEvent)
player([int]playerId, [string]action)
frame()

B.3 Distance Relationship

Lastly Version 2 of the plugin introduces a new relationship: distance. The distance relationship
works similar to XChange, YChange and ZChange but compares two joints (or one joint and a static
reference point) instead of comparing to itself. Negative values for deviation means that the
relationship is true if the distance between the specified objects (two joints or joint and static
reference point) is less than the deviation value (distance). Positive values for deviation means
that the relationship is true if the distance between the specified objects is greater than the
deviation value.