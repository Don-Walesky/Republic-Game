using UnityEngine;
using System;

public class RepublicBridge : MonoBehaviour
{
    [SerializeField] private string bootstrapMessage = "Republic simulation core ready";

    private void Start()
    {
        Debug.Log(bootstrapMessage);
    }

    public void TriggerBootstrap()
    {
        Debug.Log("Bootstrap triggered from Unity.");
    }
}
