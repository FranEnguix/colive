using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColorCommand : ICommand {

    public string Name { get; set; }

    public Color AgentColor { get; set; }

    public void Execute(Dictionary<string, GameObject> gameObjects) {
        var agent = gameObjects[Name];
        if (agent != null) {
            var entityComponent = agent.GetComponent<Entity>();
            entityComponent.ChangeColor(AgentColor);
        }
    }
}
