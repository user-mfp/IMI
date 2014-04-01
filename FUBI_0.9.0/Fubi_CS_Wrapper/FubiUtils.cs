using System;
using System.Runtime.InteropServices;

namespace FubiNET
{
    public class FubiUtils
    {
        // Options for image rendering
        public enum ImageType
        {
            /*	The possible image types
            */
            Color = 0,
            Depth,
            IR,
            Blank
        };
        public enum ImageNumChannels
        {
            /*	The number of channels in the image
            */
            C1 = 1,
            C3 = 3,
            C4 = 4
        };
        public enum ImageDepth
        {
            /*	The depth of each channel
            */
            D8 = 8,
            D16 = 16
        };
        public enum DepthImageModification
        {
            /*	How the depth image should be modified for depth differences
                being easier to distinguish by the human eye
            */
            Raw = 0,
            UseHistogram,
            StretchValueRange,
            ConvertToRGB
        };
        public enum RenderOptions
        {
            /*	The possible formats for the tracking info rendering
            */
            None = 0,
            Shapes =                0x000001,
            Skeletons =             0x000002,
            UserCaptions =          0x000004,
            LocalOrientCaptions =   0x000008,
            GlobalOrientCaptions =  0x000010,
            LocalPosCaptions =      0x000020,
            GlobalPosCaptions =     0x000040,
            Background =            0x000080,
            SwapRAndB =             0x000100,
            FingerShapes =          0x000200,
            DetailedFaceShapes =    0x000400,
            BodyMeasurements =      0x000800,
			UseFilteredValues =     0x001000,
            Default = FubiUtils.RenderOptions.Shapes | FubiUtils.RenderOptions.Skeletons | FubiUtils.RenderOptions.UserCaptions
        };
        
        public enum JointsToRender
        {
            /* IDs for the Joints to define which of them should be rendered (don't mix up with the SkeletonJoint enum!!) */
            ALL_JOINTS = -1,

            HEAD = 0x00000001,
            NECK = 0x00000002,
            TORSO = 0x00000004,
            WAIST = 0x00000008,

            LEFT_SHOULDER = 0x00000010,
            LEFT_ELBOW = 0x00000020,
            LEFT_WRIST = 0x00000040,
            LEFT_HAND = 0x00000080,

            RIGHT_SHOULDER = 0x00000100,
            RIGHT_ELBOW = 0x00000200,
            RIGHT_WRIST = 0x00000400,
            RIGHT_HAND = 0x00000800,

            LEFT_HIP = 0x00001000,
            LEFT_KNEE = 0x00002000,
            LEFT_ANKLE = 0x00004000,
            LEFT_FOOT = 0x00008000,

            RIGHT_HIP = 0x00010000,
            RIGHT_KNEE = 0x00020000,
            RIGHT_ANKLE = 0x00040000,
            RIGHT_FOOT = 0x00080000,

            FACE_NOSE = 0x00100000,
            FACE_LEFT_EAR = 0x00200000,
            FACE_RIGHT_EAR = 0x00400000,
            FACE_FOREHEAD = 0x00800000,
            FACE_CHIN = 0x01000000,

            PALM = 0x02000000,
            FINGER_ONE = 0x04000000,
            FINGER_TWO = 0x08000000,
            FINGER_THREE = 0x10000000,
            FINGER_FOUR = 0x20000000,
            FINGER_FIVE = 0x40000000
        };

	    // Maximum depth value that can occure in the depth image
	    public const int MaxDepth = 10000;
	    // Maximum number of tracked users
	    public const int MaxUsers = 15;

        public enum SkeletonJoint
        {
            HEAD			= 0,
			NECK			= 1,
			TORSO			= 2,
			WAIST			= 3,

			LEFT_SHOULDER	= 4,
			LEFT_ELBOW		= 5,
			LEFT_WRIST		= 6,
			LEFT_HAND		= 7,

			RIGHT_SHOULDER	=8,
			RIGHT_ELBOW		=9,
			RIGHT_WRIST		=10,
			RIGHT_HAND		=11,

			LEFT_HIP		=12,
			LEFT_KNEE		=13,
			LEFT_ANKLE		=14,
			LEFT_FOOT		=15,

			RIGHT_HIP		=16,
			RIGHT_KNEE		=17,
			RIGHT_ANKLE		=18,
			RIGHT_FOOT		=19,

            FACE_NOSE       =20,
            FACE_LEFT_EAR   =21,
            FACE_RIGHT_EAR  =22,
            FACE_FOREHEAD   =23,
            FACE_CHIN       =24,

			NUM_JOINTS		=25
        };

        public enum BodyMeasurement
        {
            BODY_HEIGHT = 0,
            TORSO_HEIGHT = 1,
            SHOULDER_WIDTH = 2,
            HIP_WIDTH = 3,
            ARM_LENGTH = 4,
            UPPER_ARM_LENGTH = 5,
            LOWER_ARM_LENGTH = 6,
            LEG_LENGTH = 7,
            UPPER_LEG_LENGTH = 8,
            LOWER_LEG_LENGTH = 9,
            NUM_MEASUREMENTS = 10
        };

        public enum SkeletonProfile
        {
            NONE = 1,

            ALL = 2,

            UPPER_BODY = 3,

            LOWER_BODY = 4,

            HEAD_HANDS = 5,
        };

        public enum SensorType
        {
            /** No sensor in use **/
            NONE = 0,
            /** Sensor based on OpenNI 2.x**/
            OPENNI2 = 0x01,
            /** Sensor based on OpenNI 1.x**/
            OPENNI1 = 0x02,
            /** Sensor based on the Kinect for Windows SDK 1.x**/
            KINECTSDK = 0x04
        };

        public enum FingerSensorType
	    {
			/** No sensor in use **/
			NONE = 0x0,
			/** Finger tracking with LEAP **/
			LEAP = 0x01
	    };

        /**
        * \brief A recognizer can target a specific type of sensor that provides the correct data
        */
        public enum RecognizerTarget
        {
            /** No sensor supported, usually means something went wrong **/
            NO_SENSOR = -1,
            /** A default body tracking sensor such as the Kinect with tracking software**/
            BODY_SENSOR = 0,
            /** A finger tracking sensor such as the leap motion **/
            FINGER_SENSOR = 1,
            /** All types of sensors can be used, e.g. finger count recognizer**/
            ALL_SENSORS = 3
        };

        public enum RecognitionResult
        {
            /*	Result of a gesture recognition
            */
            TRACKING_ERROR = -1,
            NOT_RECOGNIZED = 0,
            RECOGNIZED = 1,
            WAITING_FOR_LAST_STATE_TO_FINISH = 2	// Only for combinations with waitUntilLastStateRecognizersStop flag
        };
        [StructLayout(LayoutKind.Sequential)]
        public class RecognitionCorrectionHint
	    {
		    /**
                * \brief Additional information about what went wrong and how to correct it
            */
            public enum ChangeType
            {
                SPEED,
                LENGTH,
                DIRECTION,
                FINGERS
            };
            public enum ChangeDirection
            {
                DIFFERENT,
                MORE,
                LESS
            };

            [MarshalAs(UnmanagedType.I4)]
            public SkeletonJoint m_joint;
            [MarshalAs(UnmanagedType.R4)]
            public float m_dirX;
            [MarshalAs(UnmanagedType.R4)]
            public float m_dirY;
            [MarshalAs(UnmanagedType.R4)]
            public float m_dirZ;
            [MarshalAs(UnmanagedType.U1)]
            public bool m_isAngle;
            [MarshalAs(UnmanagedType.I4)]
            public ChangeType m_changeType;
            [MarshalAs(UnmanagedType.I4)]
            public ChangeDirection m_changeDirection;
            [MarshalAs(UnmanagedType.I4)]
            public int m_failedState;

            public RecognitionCorrectionHint(SkeletonJoint joint = SkeletonJoint.NUM_JOINTS, float dirX = 0, float dirY = 0, float dirZ = 0, 
                bool isAngle = false, ChangeType changeType = ChangeType.SPEED, ChangeDirection changeDir = ChangeDirection.DIFFERENT, int failedState = -1)
            {
                m_joint = joint;
                m_dirX = dirX;
                m_dirY = dirY;
                m_dirZ = dirZ;
                m_isAngle = isAngle;
                m_changeType = changeType;
                m_changeDirection = changeDir;
                m_failedState = failedState;
            }
        };

        public class StreamOptions
	    {
		    public StreamOptions(int width = 640, int height = 480, int fps = 30)
			{
                m_width =width;
                m_height =height;
                m_fps = fps;
		    }
            public void invalidate()
		    {
                m_width = -1; m_height = -1; m_fps = -1;
            }
            public bool isValid()
		    {
                return m_width > 0 && m_height > 0 && m_fps > 0;
            }

            public int m_width;
            public int m_height;
            public int m_fps;
	    };

        public class FilterOptions
	    {
		    public FilterOptions(float filterMinCutOffFrequency = 1.0f, 
			    float filterVelocityCutOffFrequency = 1.0f, float filterCutOffSlope = 0.007f)
		    {
                m_filterMinCutOffFrequency = filterMinCutOffFrequency;
                m_filterVelocityCutOffFrequency = filterVelocityCutOffFrequency;
                m_filterCutOffSlope = filterCutOffSlope;
		    }

		    
            public float m_filterMinCutOffFrequency;
            public float m_filterVelocityCutOffFrequency;
            public float m_filterCutOffSlope;
	    };

	    public class SensorOptions
	    {
		    public SensorOptions(StreamOptions depthOptions,
                StreamOptions rgbOptions, StreamOptions irOptions,
                SensorType sensorType = SensorType.OPENNI2,
                SkeletonProfile trackingProfile = SkeletonProfile.ALL,
			    bool mirrorStreams = true, bool registerStreams = true)
			{
                m_depthOptions = depthOptions;
                m_irOptions = irOptions;
                m_rgbOptions = rgbOptions;
                m_trackingProfile = trackingProfile;
                m_mirrorStreams = mirrorStreams;
                m_registerStreams = registerStreams;
                m_type = sensorType;
		    }
		    public StreamOptions m_depthOptions;
            public StreamOptions m_irOptions;
            public StreamOptions m_rgbOptions;

            public SkeletonProfile m_trackingProfile;
            public bool m_mirrorStreams;
            public bool m_registerStreams;
            public SensorType m_type;
	    };


        // Constants
        public class Math
        {
	        public const UInt32 MaxUInt32 = 0xFFFFFFFF;
            public const int MinInt32 = 0x8000000;
            public const int MaxInt32 = 0x7FFFFFFF;
	        public const float MaxFloat = 3.402823466e+38F;
            public const float MinPosFloat = 1.175494351e-38F;

            public const float Pi = 3.141592654f;
            public const float TwoPi = 6.283185307f;
            public const float PiHalf = 1.570796327f;

            public const float Epsilon = 0.000001f;
            public const float ZeroEpsilon = 32.0f * MinPosFloat;  // Very small epsilon for checking against 0.0f

            public const float NaN = 0xFFFFFFFF;

            public static float degToRad( float f ) 
            {
	            return f * 0.017453293f;
            }

            public static float radToDeg(float f) 
            {
	            return f * 57.29577951f;
            }

            public static bool rotMatToRotation(float[] mat, out float rx, out float ry, out float rz)
            {
                if (mat.Length == 9)
                {
                    rx = radToDeg((float)System.Math.Asin(-mat[7]));

                    // Special case: Cos[x] == 0 (when Sin[x] is +/-1)
                    float f = (float)System.Math.Abs(mat[7]);
                    if (f > 0.999f && f < 1.001f)
                    {
                        // Pin arbitrarily one of y or z to zero
                        // Mathematical equivalent of gimbal lock
                        ry = 0;

                        // Now: Cos[x] = 0, Sin[x] = +/-1, Cos[y] = 1, Sin[y] = 0
                        // => m[0][0] = Cos[z] and m[1][0] = Sin[z]
                        rz = radToDeg((float)System.Math.Atan2(-mat[3], mat[0]));
                    }
                    // Standard case
                    else
                    {
                        ry = radToDeg((float)System.Math.Atan2(mat[6], mat[8]));
                        rz = radToDeg((float)System.Math.Atan2(mat[1], mat[4]));
                    }
                    return true;
                }

                rx = ry = rz = 0;
                return false;
            }

            public static void quaternionToRotation(float x, float y, float z, float w, out float rx, out float ry, out float rz)
            {
                float[] mat = new float[9];
                // Calculate coefficients
                float x2 = x + x, y2 = y + y, z2 = z + z;
                float xx = x * x2, xy = x * y2, xz = x * z2;
                float yy = y * y2, yz = y * z2, zz = z * z2;
                float wx = w * x2, wy = w * y2, wz = w * z2;

                mat[0] = 1 - (yy + zz);  mat[3] = xy - wz;          mat[6] = xz + wy;
                mat[1] = xy + wz;        mat[4] = 1 - (xx + zz);    mat[7] = yz - wx;
                mat[2] = xz - wy;        mat[5] = yz + wx;          mat[8] = 1 - (xx + yy);

                rotMatToRotation(mat, out rx, out ry, out rz);
            }
        };

        public static string createCorrectionHintMsg(FubiUtils.RecognitionCorrectionHint hint)
        {
            string msg = "";
            if (hint.m_failedState > -1)
            {
                msg += "State " + hint.m_failedState + " - ";
            }
            if (hint.m_changeType == FubiUtils.RecognitionCorrectionHint.ChangeType.FINGERS)
            {
                if (hint.m_dirX > 0)
                    msg += "Please show " + hint.m_dirX.ToString("0") + " more fingers!\n";
                else
                    msg += "Please show " + hint.m_dirX.ToString("0") + " less fingers!\n";
            }
            else if (hint.m_changeType == FubiUtils.RecognitionCorrectionHint.ChangeType.DIRECTION)
            {
                string action = "move";
                if (hint.m_isAngle)
                    action = "turn";
                string direction = "";
                if (hint.m_dirX > 0.1f)
                    direction += "right ";
                else if (hint.m_dirX < -0.1f)
                    direction += "left ";
                if (hint.m_dirY > 0.1f)
                    direction += "up ";
                else if (hint.m_dirY < -0.1f)
                    direction += "down ";
                if (hint.m_dirZ > 0.1f)
                    direction += "backward ";
                else if (hint.m_dirZ < -0.1f)
                    direction += "forward ";
                msg += "Please " + action + " " + Enum.GetName(typeof(FubiUtils.SkeletonJoint), hint.m_joint) + " "
                    + direction + ":" + hint.m_dirX.ToString("0.#") + "/" + hint.m_dirY.ToString("0.#") + "/" + hint.m_dirZ.ToString("0.#") + "\n";
            }
            else
            {
                for (int dirI = 0; dirI < 3; ++dirI)
                {
                    float value = hint.m_dirX;
                    string direction = "";
                    if (dirI == 1)
                    {
                        value = hint.m_dirY;
                        if (value < 0)
                            direction = "down";
                        else
                            direction = "up";
                    }
                    else if (dirI == 2)
                    {
                        value = hint.m_dirZ;
                        if (value < 0)
                            direction = "forward";
                        else
                            direction = "backward";
                    }
                    if (System.Math.Abs(value) > 0.01f)
                    {
                        string mod = "";
                        if (hint.m_changeType == FubiUtils.RecognitionCorrectionHint.ChangeType.LENGTH)
                        {
                            if (hint.m_changeDirection == FubiUtils.RecognitionCorrectionHint.ChangeDirection.MORE)
                                mod = "more ";
                            else if (hint.m_changeDirection == FubiUtils.RecognitionCorrectionHint.ChangeDirection.LESS)
                                mod = "less ";
                        }
                        else if (hint.m_changeType == FubiUtils.RecognitionCorrectionHint.ChangeType.SPEED)
                        {
                            if (hint.m_changeDirection == FubiUtils.RecognitionCorrectionHint.ChangeDirection.MORE)
                                mod = "faster ";
                            else if (hint.m_changeDirection == FubiUtils.RecognitionCorrectionHint.ChangeDirection.LESS)
                                mod = "slower ";
                        }
                        string action = "move";
                        if (hint.m_isAngle)
                            action = "turn";
                        msg += "Please " + action + " " + Enum.GetName(typeof(FubiUtils.SkeletonJoint), hint.m_joint) + " "
                            + mod + direction + ": " + value.ToString("0.#") + "\n";
                    }
                }
            }
            return msg;
        }
    }
}
