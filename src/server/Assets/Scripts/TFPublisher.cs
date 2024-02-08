using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.SlamExample;
using UnityEngine;

public class TFPublisher : MonoBehaviour
{
    const string k_TfTopic = "/tf";
    
    [SerializeField]
    double m_PublishRateHz = 20f;
    [SerializeField]
    public Vector3 InitialPosicion;
    List <double> Lineal;
    double W;
    double m_LastPublishTimeSeconds;

    TransformTreeNode m_TransformRoot;
    ROSConnection m_ROS;
    string odom="odom";
    string tf;
    public string baseRobot="base_footprint";
    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;
    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;
    // Start is called before the first frame update
    GameObject baseFootprint;
    void Start()
    {
        GameObject robotParent = transform.parent.gameObject;

        if (robotParent != null)
        {
            // El GameObject padre contiene el identificador (por ejemplo, "robotX")
            string robotName = robotParent.name.Split('@')[0];
            tf=robotName;
            baseFootprint = robotParent.transform.Find(baseRobot).gameObject;
            // Extrae el número del nombre
            if (robotName.StartsWith("robot"))
            {
                string robotNumberStr = robotName.Substring(5); // Quita "robot"
                baseRobot = "robot" + robotNumberStr + "/" + baseRobot;
                odom= "robot" + robotNumberStr + "/" + odom;
            }
           
        }
        

        m_ROS = ROSConnection.GetOrCreateInstance();
        m_TransformRoot = new TransformTreeNode(baseFootprint,tf);
        m_ROS.RegisterPublisher<TFMessageMsg>(k_TfTopic);
        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;
        
    }
    static void PopulateTFList(List<TransformStampedMsg> tfList, TransformTreeNode tfNode, string tf)
    {
        // TODO: Some of this could be done once and cached rather than doing from scratch every time
        // Only generate transform messages from the children, because This node will be parented to the global frame
        foreach (var childTf in tfNode.Children)
        {
            tfList.Add(tfNode.ToTransformStamped(childTf,tf));

            if (!childTf.IsALeafNode)
            {
                PopulateTFList(tfList, childTf,tf);
            }
        }
    }
 
    void PublishMessage()
    {
        var tfMessageList = new List<TransformStampedMsg>();
        var Odomtf= new TransformStampedMsg(
            new HeaderMsg(0,new TimeStamp(Clock.time),"map"),
            odom,
            new TransformMsg(
                new Vector3Msg (
                    InitialPosicion.x,
                    InitialPosicion.y,
                    0
                ),
                new QuaternionMsg()));
        tfMessageList.Add(Odomtf);
        var Posetf= new TransformStampedMsg(
            new HeaderMsg(0,new TimeStamp(Clock.time),odom),
            baseRobot,
            new TransformMsg(
                new Vector3Msg (
                    baseFootprint.transform.position.z-InitialPosicion.x,
                    -baseFootprint.transform.position.x-InitialPosicion.y,
                    0
                ),
                new QuaternionMsg(
                     -baseFootprint.transform.rotation.z,
                     baseFootprint.transform.rotation.x,
                     -baseFootprint.transform.rotation.y,
                     baseFootprint.transform.rotation.w
                )));
        tfMessageList.Add(Posetf);

        PopulateTFList(tfMessageList, m_TransformRoot,tf);

        var tfMessage = new TFMessageMsg(tfMessageList.ToArray());
        m_ROS.Publish(k_TfTopic, tfMessage);
        m_LastPublishTimeSeconds = Clock.FrameStartTimeInSeconds;
    }

    void Update()
    {
        if (ShouldPublishMessage)
        {
            PublishMessage();
        }

    }}
