
namespace FubiNET
{
    public class FubiTrackingData
    {
        public FubiTrackingData()
        {
            uint numJoints = (uint)FubiUtils.SkeletonJoint.NUM_JOINTS;
            jointOrientations = new JointOrientation[numJoints];
            for (uint i = 0; i < numJoints; ++i)
                jointOrientations[i] = new JointOrientation();
            jointPositions = new JointPosition[numJoints];
            for (uint i = 0; i < numJoints; ++i)
                jointPositions[i] = new JointPosition();
        }
        public class JointPosition
        {
            public float x, y, z;
            public float confidence;
        };
        public class JointOrientation
        {
            public JointOrientation()
            {
            }
            public float rx, ry, rz;
            public float confidence;
        };

        public float[] getArray()
        {
            uint numJoints = (uint)FubiUtils.SkeletonJoint.NUM_JOINTS;
            float[] skeleton = new float[8 * numJoints];
            for (uint i = 0; i < numJoints; ++i)
            {
                uint startIndex = i * 8;
                
                skeleton[startIndex] = jointPositions[i].x;
                skeleton[startIndex + 1] = jointPositions[i].y;
                skeleton[startIndex + 2] = jointPositions[i].z;
                skeleton[startIndex + 3] = jointPositions[i].confidence;

                skeleton[startIndex + 4] = jointOrientations[i].rx;
                skeleton[startIndex + 5] = jointOrientations[i].ry;
                skeleton[startIndex + 6] = jointOrientations[i].rz;
                skeleton[startIndex + 7] = jointOrientations[i].confidence;
            }
            return skeleton;
        }

        public JointOrientation[] jointOrientations;
        public JointPosition[] jointPositions;

        public double timeStamp;
    }

    public class FubiAccelerationData
    {
        public FubiAccelerationData()
        {
            uint numJoints = (uint)FubiUtils.SkeletonJoint.NUM_JOINTS;
            jointAccelerations = new JointAcceleration[numJoints];
            for (uint i = 0; i < numJoints; ++i)
                jointAccelerations[i] = new JointAcceleration();
        }
        public class JointAcceleration
        {
            public float x, y, z;
            public float confidence;
        };

        public float[] getArray()
        {
            uint numJoints = (uint)FubiUtils.SkeletonJoint.NUM_JOINTS;
            float[] skeleton = new float[4 * numJoints];
            for (uint i = 0; i < numJoints; ++i)
            {
                uint startIndex = i * 4;

                skeleton[startIndex] = jointAccelerations[i].x;
                skeleton[startIndex + 1] = jointAccelerations[i].y;
                skeleton[startIndex + 2] = jointAccelerations[i].z;
                skeleton[startIndex + 3] = jointAccelerations[i].confidence;
            }
            return skeleton;
        }

        public JointAcceleration[] jointAccelerations;

        public double timeStamp;
    }
}
