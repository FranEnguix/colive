using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.Core;
using UnityEngine;
using RosOdom = RosMessageTypes.Nav.OdometryMsg;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.Serialization;

public class ROSOdomPublish : MonoBehaviour
{
    string topic;
    [SerializeField]
    double m_PublishRateHz = 20f;
    [SerializeField]
    public string FrameIdOption = "base_link";
    string childFrame = "odom";
    ROSConnection m_Ros;
    double m_LastPublishTimeSeconds;
    

    GameObject baseFootprint;
    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;
    void Start()
    {
        GameObject robotParent = transform.parent.gameObject;

        if (robotParent != null)
        {
            // El GameObject padre contiene el identificador (por ejemplo, "robotX")
            string robotName = robotParent.name.Split('@')[0];
            baseFootprint = robotParent.transform.Find(FrameIdOption).gameObject;
            // Extrae el número del nombre
            if (robotName.StartsWith("robot"))
            {
                string robotNumberStr = robotName.Substring(5); // Quita "robot"
                FrameIdOption = "robot" + robotNumberStr + "/" + FrameIdOption;
                childFrame= "robot" + robotNumberStr + "/" + childFrame;
                topic="robot" + robotNumberStr + "/odom" ;
            }
            
        }

        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<OdometryMsg>(topic);
        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;
    }

    // Update is called once per frame
    void PublishMessage()
    {
    var timestamp = new TimeStamp(Clock.time);

    var msg=new RosOdom{
        child_frame_id=childFrame,
        header = new HeaderMsg
        {
                frame_id = FrameIdOption,
                stamp = new TimeMsg
                {
                    sec = timestamp.Seconds,
                    nanosec = timestamp.NanoSeconds,
                }
        },
        pose = new PoseWithCovarianceMsg
        {
            pose= new PoseMsg
            {
                position=new PointMsg
                {
                    y=-baseFootprint.transform.position.x,
                    x=baseFootprint.transform.position.z,
                },
                orientation = new QuaternionMsg
                {   
                    x=-baseFootprint.transform.rotation.z,
                    y=baseFootprint.transform.rotation.x,
                    z=-baseFootprint.transform.rotation.y,
                    w=baseFootprint.transform.rotation.w
                }
            },
            
        },
        twist = new TwistWithCovarianceMsg 
        {}
        };

    m_Ros.Publish(topic, msg);
    m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }

    void Update()
    {
        if (ShouldPublishMessage)
        {
            PublishMessage();
        }

    }

}
