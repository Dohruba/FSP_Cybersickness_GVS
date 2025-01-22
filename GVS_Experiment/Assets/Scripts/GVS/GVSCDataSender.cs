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

    void Start()
    {
        UnitySerialPort.Instance.OpenSerialPort();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pinging port");
            SendMessageToSerialPort("Hello from Unity!");
        }
    }
    private void OnEnable()
    {
        SubscribeToTrackers();
    }
    private void OnDisable()
    {
        UnsubscribeFromTrackers();
    }

    private void HandleRotation(Vector3 vector, AccelerationTypes types)
    {
        if (!rotationIsSending || vector.y == 0) return;
        SendValueToGVS(vector.y);
    }

    private void HandleAcceleration(Vector3 direction, AccelerationTypes type)
    {
        if (!accIsSending) return;
        SendValueToGVS(direction);
    }

    void SendValueToGVS(float value)
    {
        string valueString = value.ToString();
        SendMessageToSerialPort(valueString);
    }
    void SendValueToGVS(Vector3 value)
    {
        string valueString = value.ToString();
        SendMessageToSerialPort(valueString);
    }
    void SendMessageToSerialPort(string message)
    {
        if (UnitySerialPort.Instance != null && UnitySerialPort.Instance.SerialPort.IsOpen)
        {
            UnitySerialPort.Instance.SendSerialDataAsLine(message);
        }
        else
        {
            Debug.LogWarning("Serial port is not open!");
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
    private void UnsubscribeFromTrackers()
    {
        foreach (var tracker in gvsInfluencers)
        {
            if (tracker != null)
            {
                tracker.Unsubscribe(HandleAcceleration);
            }
        }
        rotator.Unsubscribe(HandleRotation);
    }

}