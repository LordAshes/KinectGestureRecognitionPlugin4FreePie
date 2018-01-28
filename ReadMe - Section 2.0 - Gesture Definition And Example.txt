2.0 GESTURE DEFINITION

Regardless if gesture definitions are loaded automatically by the plugin settings, loaded from an XML
file using the corresponding script method or created dynamically at runtime, the definition is the
same.

Gestures are in a list of GestureSequences. Each GestureSequence in the list defines a unique gesture
which can be recognized by the plugin.

Each GestureSequence contains the gesture name, the gesture timeout period as well as  as a list of
GestureSequenceStep.

Each GestureSequenceStep contains a list of success JointRelationship and failure JointRelationship.

Finally each JointRelationship contains information about the relationship including the acting joint,
the relation, the relative and possibly a deviation.

GESTURE DEFINITION EXAMPLE:

As an example, consider the process of creating a Gesture of raising the left arm up and then back
down...

First we would add a new GestureSequence to the Gesture list. The GestureSequence name might be
"Raise Left Hand" and the timeout may be 5000 (milliseconds).

Next we add a new GestureSequenceStep. This first GestureSequenceStep will define the starting
conditions for the gesture.

Now we add a bunch of JointRelationship to the success conditions. We might add one JointRelationship
for HandLeft to be Below ShoulderLeft and a second JointRelationship for HandLeft to be LeftOf
ShoulderLeft.

Lastly we may add one or more JointRelationship to the failure conditions. In this case we might add
a JointRelationship for HandLeft Right Of ShoulderLeft. This would mean that the gesture woudl fail
if the user's HandLeft moves to the right of ShoulderLeft.

Now we add the next GestureSequenceStep. This will define the next stage in our gesture.

Similar to before we add a bunch of success condition JointRelationship. In this case we add a
JointRelationship for HandLeft to be Above ShoulderLeft and a second JointRelationship for HandLeft
to be LeftOf ShoulderLeft.

Once again we add a failure condition JointRelationship which in this case can remain, as before,
HandLeft RightOf ShoulderLeft.

This would be enough for a gesture that detects the raising of the left arm. However, if we want to
include lowering the arm as part of the gesture, we would need to add one more GestureSequenceStep
which would add a bunch of JointRelationship, in this case, identical to the first GestureSequence
requiring the arm to be lowered back to the starting position in order for the gesture to be complete.
