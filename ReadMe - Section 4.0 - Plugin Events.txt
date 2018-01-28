4.0 PLUGIN EVENTS

The plugin provides two events that can be subscribed to:

4.1 Update Event

The update event is triggered when one or more gestures are recognized. Typically a script subscribes
to this method to determine when to read the recognized gestures dictionary or recognized gestures
list using either GetRecognizedGesture(...) or GetRecognizedGestureByPlayerId(...).

As is indicated in the description of these methods (in Section 5) these methods resolve to lists of
gestures in case multiple gestures are recognized between readings. It should also be assumed that
there may not be 1 to 1 relationship between the number of gestures recognized and the number of times
that this event is raised. It is possible that multiple gestures being recognized within a short time
period of each other may generate a single raising of this event.

4.2 Processing Event

The processing event is triggered when a processing step has been completed. Typically a script
subscribes to this method to determine when use the GetRecognitionProcessEvents(...) method.
In most cases it is not necessary to read the processing events since, in most cases, teh script is
only interested in gestures that are completed (which are notified using the updates event).
However, in some cases such as troubleshooting a script, troubleshooting a gesture definition or
even trying to identify issues the user may be having with using the script, subscribing to this
event can provide additional information about the processing of the gesture recognition parser. 