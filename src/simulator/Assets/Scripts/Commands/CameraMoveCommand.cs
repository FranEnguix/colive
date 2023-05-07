using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveCommand : ICommand {
    public string Name { get; set; }

    public int CameraIndex { get; set; }

    public Vector3 MoveAxis { get; set; }

    public float Units { get; set; }

    public void Execute(Dictionary<string, GameObject> gameObjects) {
        var agent = gameObjects[Name];
        if (agent != null) {
            var entityComponent = agent.GetComponent<Entity>();
            entityComponent.CameraMove(CameraIndex, MoveAxis, Units);
        }
    }
}
