using S22.Xmpp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateArtifactCommand : ICommand
{
    public string Name { get; set; }

    public Jid Jid { get; set; }

    public string ArtifactPrefab { get; set; }

    public string SpawnerName { get; set; }

    public Vector3 StarterPosition { get; set; }

    public void Execute(Dictionary<string, GameObject> gameObjects) {
    }

    public bool IsInstantiatedByCoordinates () {
        return !(SpawnerName != null && SpawnerName.Length > 0);
    }

}
