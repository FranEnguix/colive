using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayDataCommand : ICommand
{
    public string Name { get; set; }

    public string[] Data { get; set; }

    public void Execute(Dictionary<string, GameObject> gameObjects) {
        GameObject gameObj = gameObjects[Name];
        var entityComponent = gameObj.GetComponent<Artifact>();
        entityComponent.SetDisplayedData(String.Join(" ", Data));
    }

}
