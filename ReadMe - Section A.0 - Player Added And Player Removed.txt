SECTION A.0: Player Added And Player Removed

The processing event now includes messages when players are added and removed from the Kinect sensor
field of view. 

The Kinect Sensor provides feedback on the position of skeletons (players) and their joints in sets
called frames. Each frame the information regarding each identified player is updated. However in some
cases a frame is provided in which one or more of the player's information is missing. Without looking
into the details of the hardware and drivers, it is unclear why this happens but this makes it more
difficult to determine when a player has actually left the sensor range because missing information
could be an actual player not being in sensor range or a temporary frame missing the player info.

In order to prevent a bunch of false player remove messages (followed by player added messages) the
plugin requires a player to be missing from the player information for a number of consecutive frames
before a player removed message is issued. This value can be configured in the plugin settings.

If this value is set too low, the plugin is likely to generate false player removal messages (which
will be followed by player added messages). If the value is set too high false player removal messages
will be avoided but it will take longer for the plugin to generate the player removal message.
