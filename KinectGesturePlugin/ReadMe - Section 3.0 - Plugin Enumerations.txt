3.0 PLUGIN ENUMERATIONS

The "KinectJoint" namespace enumerates the available joints that can be used in gesture definitions.

        AnkleLeft
        AnkleRight
        ElbowLeft
        ElbowRight
        FootLeft
        FootRight
        HandLeft
        HandRight
        HipCenter
        HipLeft
        HipRight
        KneeLeft
        KneeRight
        ShoulderCenter
        ShoulderLeft
        ShoulderRight
        Spine
        WristLeft
        WristRight

The "KinectJointRelationship" namespace enumerates the available relationships that joints (and
possibly static reference points) can have between each other.

        Above
        Behind
        Below
        InfrontOf
        LeftOf
        RightOf
	Distance*
        XChange
        YChange
        ZChange	
        None**

* Added as of Version 2. See Section 6 for details.
** This relationship is used internally and should not be used in gesture configuration.

