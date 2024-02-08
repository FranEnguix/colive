using System.Collections.Generic;
using RosMessageTypes.Geometry;
using Unity.Robotics.Core;
using Unity.Robotics.UrdfImporter;
using UnityEngine;

namespace Unity.Robotics.SlamExample
{
    class TransformTreeNode
    {
        public readonly GameObject SceneObject;
        public readonly List<TransformTreeNode> Children;
        public Transform Transform => SceneObject.transform;
        public string name => SceneObject.name;
        public bool IsALeafNode => Children.Count == 0;
        string prefix; // Variable para almacenar el valor personalizado

        public TransformTreeNode(GameObject sceneObject, string prefix)
        {
            SceneObject = sceneObject;
            Children = new List<TransformTreeNode>();
            this.prefix = prefix; // Almacena el valor personalizado
            PopulateChildNodes(this);
        }

        public TransformStampedMsg ToTransformStamped(TransformTreeNode node,string prefix)
        {
            return node.Transform.ToROSTransformStamped(Clock.time, prefix); // Utiliza el valor personalizado
        }

        static void PopulateChildNodes(TransformTreeNode tfNode)
        {
            var parentTransform = tfNode.Transform;
            for (var childIndex = 0; childIndex < parentTransform.childCount; ++childIndex)
            {
                var childTransform = parentTransform.GetChild(childIndex);
                var childGO = childTransform.gameObject;

                // Si el objeto del juego tiene un UrdfLink adjunto, es un enlace en el árbol de transformación
                if (childGO.TryGetComponent(out UrdfLink _))
                {
                    var childNode = new TransformTreeNode(childGO, "hola"); // Pasa el prefijo y el valor personalizado
                    tfNode.Children.Add(childNode);
                }
            }
        }
    }
}
