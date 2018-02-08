6.0 XCHANGE, YCHANGE, ZCHANGE and DISTANCE

6.1 XCHANGE, YCHANGE, ZCHANGE

Unlike other KinectJointRelationship members which establish a relation between the current
positions of the actor (joint) and relative (joint or reference point), the XChange, 
YChange and ZChange establish a relation between the current and previous position for the
actor. When using these relationships, the actor and the relative should be the same.

In these cases, the deviation parameter is used (which should be 0 for other relationships)
to not only indicate the threshold value for the change but also the direction.

For example:

A relationship of (actor=)HandLeft (relation=)XChange (relative=)HandLeft (deviatin=)-30
would be true if the HandLeft has move at least 30 units to the left (negative deviation
for an XChange).

It should be noted that the previous reference point (not to be confused with Static
Reference Points) for XChange, YChange and ZChange relationships is the position at which
the last step was passed. This means that XChange, YChange and ZChange should not be used
as part of the success or failure conditions of the first step in a gesture sequence
(since there would not be any previous step reference to compare to).

6.2 DISTANCE

The Distance KinectJointRelationship refers to the distance between the current position
if the actor (joint) and relative (joint or reference point). The provided deviation value
not only indicates the reference distance but also the success condition regarding the
deviation distance. If the deviation is negative then the relationship will be true if the
distance between the joints is less than (the absolute value of) the deviation. If the
deviation value is positive then the relationship will be true if the distance is more than
the deviation value. For example:

HandLeft Distance HipLeft -30 means true if HandLeft is less than 30 away from HipLeft
HandLeft Distance HipLeft 90 means true if HandLeft is more than 90 away from HipLeft

