using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using S22.Xmpp;

public class Artifact : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    [SerializeField] TextMeshPro data;

    private Jid jid;

    void Start() {
        data.text = "";
    }

    void Update() {
        
    }

    public void ChangeColor(Color color) {
        GetComponent<Renderer>().material.color = color;
    }

    public void SetDisplayName(string displayName) {
        text.text = displayName;
    }

    public void SetDisplayedData(string displayedData) {
        data.text = displayedData;
    }

    public Jid Jid {
        get { return jid; }
        set { jid = value; }
    }
}
