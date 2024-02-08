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

public class ROSCostPublish : MonoBehaviour
{
    string topic = "CostRoute";
    [SerializeField]
    double m_PublishRateHz = 20f;
    float Id = 0;
    ROSConnection m_Ros;
    double m_LastPublishTimeSeconds;

    public string BaseName="base_footprint";

    float Xini;

    float Yini;

    float Xfin;
    float Yfin;
    float Distance=0;


    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;
    GameObject baseFootprint;

    void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<Float64MultiArrayMsg>(topic);
        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;

        // Obtén el GameObject padre (RobotX) del GameObject actual
        GameObject robotParent = transform.parent.gameObject;

        if (robotParent != null)
        {
            // El GameObject padre contiene el identificador (por ejemplo, "robotX")
            string robotName = robotParent.name.Split('@')[0];
            // Extrae el número del nombre
            if (robotName.StartsWith("robot"))
            {
                string robotNumberStr = robotName.Substring(5); // Quita "robot"
                if (float.TryParse(robotNumberStr, out float robotNumber))
                {
                    Id = robotNumber;
                }
            }
            baseFootprint = robotParent.transform.Find(BaseName).gameObject;
        }
        
        Xfin=baseFootprint.transform.position.z;

        Yfin=-baseFootprint.transform.position.x;
    }
    void PublishMessage()
    {
        var timestamp = new TimeStamp(Clock.time);

        CalculateDistance();

        var msg = new Float64MultiArrayMsg
        {
            data = new double[] { Id, Distance},
        };
        m_Ros.Publish(topic, msg);
        m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }
    void CalculateDistance (){
        
        Xini=Xfin;

        Yini=Yfin;
        
        Xfin=baseFootprint.transform.position.z;

        Yfin=-baseFootprint.transform.position.x;

        float Dx = Xini-Xfin;
        float Dy= Yini-Yfin;
        Distance=(float)Math.Sqrt(((Dx)*(Dx))+((Dy)*(Dy)));
    }
    void Update()
    {
        if (ShouldPublishMessage)
        {
            PublishMessage();
        }
    }
}