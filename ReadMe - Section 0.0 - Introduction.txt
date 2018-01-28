			-------------------------------------------------
			- Kinect Gesture Recognition Plugin for FreePie -
			-------------------------------------------------
			                    Version 1.0

The Kinect Gesture Recognition Plugin focuses on being able to define simple to complex gestures using
relationships between joints and, optionally, startic reference points. Gesture sequences can be loaed
from an XML file or created dynamically at runtime. The gesture recognition system is optimized to only
evaluate the joints involved in gestures as opposed to evaluatin all relationships between all joints.

The plugin is designed to make use of the gesture recognition plugin as easy as possible hiding the
complex processing from the user and exposing only easy to use methods for starting, defining and
stopping the gesture recognition process.

Installation Of Plugin In FreePie:

1. Once compiled, place the "KinectGesturePlugin.dll" in the FreePie\Plugins folder.
2. Place the provided "Microsoft.Kinect.dll" in the FreePie folder.
3. Optional: Copy Gexture.xml to the FreePie folder. This is a sample gesture file.
4. Run FreePie.

Note: The Microsoft.Kinect.dll comes from Microsoft's Development Toolkit for Kinect Sensors. I was not written as part of this project.
