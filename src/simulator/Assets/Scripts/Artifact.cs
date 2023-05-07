using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Artifact : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    [SerializeField] TextMeshPro data;

    void Start() {
        text.text = name;
        data.text = "";
    }

    void Update() {
        
    }
}
