using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter.Control;

namespace RosSharp.Control1
{
    public enum ControlMode { Keyboard, ROS};

    public class AGVController4Wheels : MonoBehaviour
    {
        public GameObject wheel1;
        public GameObject wheel2;
        public GameObject wheel3;
        public GameObject wheel4;
        public ControlMode mode = ControlMode.ROS;

        private ArticulationBody wA1;
        private ArticulationBody wA2;
        private ArticulationBody wA3;
        private ArticulationBody wA4;

        public float maxLinearSpeed = 2; //  m/s
        public float maxRotationalSpeed = 1;//
        public float wheelRadius = 0.033f; //meters
        public float trackWidth = 0.288f; // meters Distance between tyres
        public float forceLimit = 10;
        public float damping = 10;

        public float ROSTimeout = 0.5f;
        private float lastCmdReceived = 0f;

        ROSConnection ros;
        private RotationDirection direction;
        private float rosLinear = 0f;
        private float rosAngular = 0f;

        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            wA3 = wheel3.GetComponent<ArticulationBody>();
            wA4 = wheel4.GetComponent<ArticulationBody>();
            SetParameters(wA1);
            SetParameters(wA2);
            SetParameters(wA3);
            SetParameters(wA4);
            ros = ROSConnection.GetOrCreateInstance();
            string robotName = gameObject.name.Split('@')[0];
            if (robotName.StartsWith("robot"))
            {
                string robotNumberStr = robotName.Substring(5); // Quita "Robot"
                string cmdVelTopic = "robot" + robotNumberStr + "/cmd_vel";
                
                ros.Subscribe<TwistMsg>(cmdVelTopic, ReceiveROSCmd);
            }
        }

        void ReceiveROSCmd(TwistMsg cmdVel)
        {
            rosLinear = (float)cmdVel.linear.x;
            rosAngular = (float)cmdVel.angular.z;
            lastCmdReceived = Time.time;
        }

        void FixedUpdate()
        {
            if (mode == ControlMode.Keyboard)
            {
            }
            else if (mode == ControlMode.ROS)
            {
                ROSUpdate();
            }     
        }

        private void SetParameters(ArticulationBody joint)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.forceLimit = forceLimit;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
        {
            ArticulationDrive drive = joint.xDrive;
            if (float.IsNaN(wheelSpeed))
            {
                drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
            }
            else
            {
                drive.targetVelocity = wheelSpeed;
            }
            joint.xDrive = drive;
        }

        private void ROSUpdate()
        {
            if (Time.time - lastCmdReceived > ROSTimeout)
            {
                rosLinear = 0f;
                rosAngular = 0f;
            }
            RobotInput(rosLinear, rosAngular);
        }

        private void RobotInput(float speed, float rotSpeed) // m/s and rad/s
        {

            float girodcha = 0.15f;
            float giroizq=0.15f;
            if (speed > maxLinearSpeed)
            {
                speed = maxLinearSpeed;
            }
            if (rotSpeed > maxRotationalSpeed)
            {
                rotSpeed = maxRotationalSpeed;
            }
            float wheel1Rotation = (speed / wheelRadius);
            float wheel2Rotation = wheel1Rotation;
            float wheel3Rotation = wheel1Rotation;
            float wheel4Rotation = wheel1Rotation;
            float wheelSpeedDiff = ((rotSpeed * trackWidth) / wheelRadius);
            if (rotSpeed != 0)
            {
                wheel1Rotation = (wheel1Rotation - (wheelSpeedDiff / giroizq)) * Mathf.Rad2Deg;
                wheel2Rotation = (wheel2Rotation - (wheelSpeedDiff / giroizq)) * Mathf.Rad2Deg;
                wheel3Rotation = (wheel3Rotation + (wheelSpeedDiff / girodcha)) * Mathf.Rad2Deg;
                wheel4Rotation = (wheel4Rotation + (wheelSpeedDiff / girodcha)) * Mathf.Rad2Deg;
            }
            else
            {
                wheel1Rotation *= Mathf.Rad2Deg;
                wheel2Rotation *= Mathf.Rad2Deg;
                wheel3Rotation *= Mathf.Rad2Deg;
                wheel4Rotation *= Mathf.Rad2Deg;
            }
            SetSpeed(wA1, wheel1Rotation);
            SetSpeed(wA2, wheel2Rotation);
            SetSpeed(wA3, wheel3Rotation);
            SetSpeed(wA4, wheel4Rotation);
        }
    }
}
