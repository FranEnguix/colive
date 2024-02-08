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

public class ROSTransformTreePublisher : MonoBehaviour
{
    const string k_TfTopic = "/tf";
    
    [SerializeField]
    double m_PublishRateHz = 20f;
    [SerializeField]
    List<string> m_GlobalFrameIds = new List<string> { "map", "odom" };
    [SerializeField]
    GameObject m_RootGameObject;

    public string baseRobot="base_footprint";
    double m_LastPublishTimeSeconds;

    TransformTreeNode m_TransformRoot;
    ROSConnection m_ROS;
    string child_frame_id="odom";
    double PublishPeriodSeconds => 1.0f / m_PublishRateHz;

    bool ShouldPublishMessage => Clock.NowTimeInSeconds > m_LastPublishTimeSeconds + PublishPeriodSeconds;

    // Start is called before the first frame update
    void Start()
    {
        if (m_RootGameObject == null)
        {
            Debug.LogWarning($"No GameObject explicitly defined as {nameof(m_RootGameObject)}, so using {name} as root.");
            m_RootGameObject = gameObject;
        }

        m_ROS = ROSConnection.GetOrCreateInstance();
        m_TransformRoot = new TransformTreeNode(m_RootGameObject,"hola");
        m_ROS.RegisterPublisher<TFMessageMsg>(k_TfTopic);
        m_LastPublishTimeSeconds = Clock.time + PublishPeriodSeconds;
    }

    static void PopulateTFList(List<TransformStampedMsg> tfList, TransformTreeNode tfNode)
    {
        // TODO: Some of this could be done once and cached rather than doing from scratch every time
        // Only generate transform messages from the children, because This node will be parented to the global frame
        foreach (var childTf in tfNode.Children)
        {


            tfList.Add(tfNode.ToTransformStamped(childTf,"holaS"));
            if (!childTf.IsALeafNode)
            {
                PopulateTFList(tfList, childTf);
            }
        }
    }

    void PublishMessage()
    {
        var tfMessageList = new List<TransformStampedMsg>();

        // In case there are multiple "global" transforms that are effectively the same coordinate frame, 
        // treat this as an ordered list, first entry is the "true" global  

        for (var i = 1; i < m_GlobalFrameIds.Count; ++i)
        {
            if (m_GlobalFrameIds[i]=="odom"){
            var tfGlobalToGlobal = new TransformStampedMsg(
                new HeaderMsg(1,new TimeStamp(Clock.time), m_GlobalFrameIds[i - 1]),
                m_GlobalFrameIds[i],
                // Initializes to identity transform
                new TransformMsg(
                    new Vector3Msg (
                    20,
                    34,
                    0),
                    new QuaternionMsg()));
            tfMessageList.Add(tfGlobalToGlobal);}
            else{       
            var tfGlobalToGlobal = new TransformStampedMsg(
                new HeaderMsg(1,new TimeStamp(Clock.time), m_GlobalFrameIds[i - 1]),
                m_GlobalFrameIds[i],
                // Initializes to identity transform
                new TransformMsg());  
                tfMessageList.Add(tfGlobalToGlobal);
            }
        }

        var Posetf= new TransformStampedMsg(
            new HeaderMsg(0,new TimeStamp(Clock.time),"odom"),
            baseRobot,
            new TransformMsg(
                new Vector3Msg (
                    m_RootGameObject.transform.position.z-20,
                    -m_RootGameObject.transform.position.x-34,
                    0
                ),
                new QuaternionMsg(
                     m_RootGameObject.transform.rotation.x,
                     m_RootGameObject.transform.rotation.y,
                     m_RootGameObject.transform.rotation.z,
                     m_RootGameObject.transform.rotation.w
                )));
        tfMessageList.Add(Posetf);
        PopulateTFList(tfMessageList, m_TransformRoot);

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

    }
}
