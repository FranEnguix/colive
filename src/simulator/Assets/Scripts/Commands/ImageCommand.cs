using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageCommand : ICommand {
    public string Name { get; set; }
    public int CameraIndex { get; set; }
    public float CaptureFrequency { get; set; }

    public void Execute(Dictionary<string, GameObject> gameObjects) {
        var entity = gameObjects[Name].GetComponent<Entity>();
        entity.LaunchTakePicture(CameraIndex, CaptureFrequency);
    }
}
