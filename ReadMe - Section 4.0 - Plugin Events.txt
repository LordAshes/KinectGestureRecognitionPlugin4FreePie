4.0 PLUGIN EVENTS

The plugin provides 4 different events that can be subscribed to depending on the detail of
information that is desired. Typically users only need to subscribe to the updates event but
the additional events are provided if more complex operation is desired.

4.1 Update Event

The update events is the event that most users of the plugin will want to subscribe to because
most users are only interested about when a gesture is completed. Note, however, that unlike
some plugins which trigger the update event periodically this plugin is truely event based and
thus the update event is only triggered when a gesture is recognized. If the user needs a more
periodic event either the processing or frame event should be used.

The update event provides the playerId (as an integer) and the gesture name (as a string) as
part of the update function data. No need to make function calls to obtain the date. Thus the
correct syntax for using the update event would be something similar to:

	update(p,g):
	  diagnostic.debug("Player %s completed gesture %s" % (p,g))
	
	starting:
	KinectGesture.update += update
	KinectGesture.RecognitionStart()
	
4.2 Processing Event

The processing event provides access to more detailed information including such information as the
gesture step in the gesture sequenece at which the gesture is currently at, information about gesture
timeouts, and other similar processing information. Typically a user of the plugin does not need
to subscribe to this event when using the plugin. This event is more useful when troubleshooting
gesture configurations because it allows the user to determine which part of the gesture sequence is
causing problems.

The processing event provides the processing event (as a string) as part of the processing function
data. No need to make function calls to obtain the date. Thus the correct syntax for using the event
would be something similar to:

	processing(e):
	  diagnostic.debug("The following events has occured: %s" % e)
	
	starting:
	KinectGesture.processing += processing
	KinectGesture.RecognitionStart()

4.3 Player Event

The player event provides information on recognized new players and recognized removal of players.
In most cases users of the plugin don't need this information but in cases the addition and/or
removal of players is significant these events can be used to determine such occurances. As
discussed in Section A of this documentation, to avoid (or at least reduce) falesly identifying a
player removal, the plugin uses frame counting mechanism. If the Kinect sensor does not provide
information about a player for the configured number of frames the player is considred removed
and thus the plugin will generate a removed message. If the Kinect sensor reports some activity
for the player the frame count, for that player, is reset thus avoiding the removal message.
The number of frames needed can be configured through the plugin settings.

This events generates 4 types of messages: added, removed, active, inactive. The first two messages
indicate that the plugin has determined that a player has been added or removed. The other two
messages are the processing logic for making that determination. When a player goes not show any
activity in the Kinect sensor frame, the player will generate an inactive message which will count
frames until the player becomes active or the count reaches the coutn necessary for player removal.

The player event provides the playerId (as an integer) and the action (as a string) as part of the
player function data. No need to make function calls to obtain the date. Thus the correct syntax
for using the player event would be something similar to:

	player(p,a):
	  diagnostic.debug("Player %s %s" % (p,a))
	
	starting:
	KinectGesture.player += player
	KinectGesture.RecognitionStart()

Do to fairly regular events such a gesture timeout, gesture step successes, gesture step failures,
and so on this event is tripped much more frequently than the update event and thus can be use
when a more periodic event is required. However, since it is still driven by processing events the
intervals at which it will be tripped are unlikley to be regular.

4.4 Frame Event

The frame event is tripped after each evaluation of relationships. In most cases users will not
need to subscribe to this event but it can be useful when using the GetPlayerInfo method and/or
the GetJointInfo method to do some custom processing. Unlike the other events this event does not
provide any additonal information in the corresponding event handler function. As such the synatx
for this event would be something like:

	frame():
	  diagnostic.debug("Another Frame Calculated")
	
	starting:
	KinectGesture.frame += frame
	KinectGesture.RecognitionStart()

This event will be tripped more often than any of the other events and will be tripped on fairly
regular (periodic) intervals. 