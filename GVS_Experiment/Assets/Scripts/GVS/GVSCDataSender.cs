using System;
using Unity.VisualScripting;
using UnityEngine;

public class GVSCDataSender : MonoBehaviour
{
    [SerializeField]
    private GVSReporterBase[] gvsInfluencers;
    [SerializeField]
    private SmoothRotation rotator;
    public bool rotationIsSending;
    public bool accIsSending;

    private void Awake()
    {
        SubscribeToTrackers();
    }
    void Start()
    {
        // Example of opening the serial port when the game starts
        UnitySerialPort.Instance.OpenSerialPort();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Sending data");
            SendMessageToSerialPort("Hello from Unity!");
        }
    }

    private void SubscribeToTrackers()
    {
        foreach (var tracker in gvsInfluencers)
        {
            if (tracker != null)
            {
                tracker.Subscribe(HandleAcceleration);
            }
        }
        rotator.Subscribe(HandleRotation);
    }

    private void HandleRotation(Vector3 vector, AccelerationTypes types)
    {
        if (!rotationIsSending) return;
        Debug.Log("Sending rotation data");
        if (vector.y > 0)
        {
            Debug.Log("Rotating right");
            SendMessageToSerialPort("Rotating right: " + vector.ToString());
        }
        else
        {
            Debug.Log("Rotating left");
            SendMessageToSerialPort("Rotating left: " + vector.ToString());
        }
    }

    private void HandleAcceleration(Vector3 direction, AccelerationTypes type)
    {
        if (!accIsSending) return;
        Debug.Log("Sending lin acc data");
        SendMessageToSerialPort("Acc Direction: " + direction.ToString());
    }

    void SendMessageToSerialPort(string message)
    {
        if (UnitySerialPort.Instance != null && UnitySerialPort.Instance.SerialPort.IsOpen)
        {
            UnitySerialPort.Instance.SendSerialDataAsLine(message);
            Debug.Log("Data Sent: " + message);
        }
        else
        {
            Debug.LogWarning("Serial port is not open!");
        }
    }

}