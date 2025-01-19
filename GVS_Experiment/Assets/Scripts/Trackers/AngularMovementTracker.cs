using System;
using System.Collections.Generic;
using UnityEngine;

public class AngularMovementTracker : GVSReporterBase
{
    // Variables for tracking angular movement
    public Transform headTransform;
    private Quaternion previousRotation;
    private Quaternion currentRotation;
    private Vector3 previousAngularVelocity;
    private Vector3 currentAngularVelocity;
    private Vector3 angularAcceleration;
    private float angularSpeed;
    private bool isRecording;
    private List<string> data;
    private string fileName = "AngularMovement";
    private string fileHeaders = "id,s,deg/s,deg/s^2";
    private int batchSize = 1000;
    private AccelerationTypes accType = AccelerationTypes.Angular;
    private string id;


    private event Action<List<string>> OnTracked;
    private event Action<Vector3, AccelerationTypes> OnAccelerate;

    private void Start()
    {
        id = ExperimentManager.GetGuid();
        data = new List<string>();
        previousRotation = headTransform.rotation;
        previousAngularVelocity = Vector3.zero;
        data.Add(fileName);
        data.Add(fileHeaders);
    }

    private void FixedUpdate()
    {
        if (isRecording)
        {
            Track();
            if (data.Count > batchSize) // +1 for the fileName
            {
                TriggerStringListeners();
            }
        }
    }

    // Track angular movement
    public override void Track()
    {
        currentRotation = headTransform.rotation;
        currentAngularVelocity = CalculateAngularVelocity(previousRotation, currentRotation, Time.deltaTime);
        angularSpeed = currentAngularVelocity.magnitude;

        angularAcceleration = (currentAngularVelocity - previousAngularVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);

        OnAccelerate?.Invoke(angularAcceleration, accType);

        string line = $"{id}{Time.time:F4},{angularSpeed:F4},{angularAcceleration.magnitude:F4}";
        data.Add(line);

        previousRotation = currentRotation;
        previousAngularVelocity = currentAngularVelocity;
    }

    public override void StopRecording()
    {
        // Ensure any remaining data is saved
        if (data.Count > 1)
        {
            TriggerStringListeners();
        }
        isRecording = false;
    }

    public override bool IsRecording()
    {
        return isRecording;
    }

    public override void StartRecording()
    {
        isRecording = true;
    }

    public override void Subscribe(Action<List<string>> subscriber)
    {
        Debug.Log("Subscribing...");
        OnTracked += subscriber;
    }

    public override void Unsubscribe(Action<List<string>> subscriber)
    {
        OnTracked -= subscriber;
    }

    public override void TriggerStringListeners()
    {
        Debug.Log("TriggerListeners...");
        OnTracked?.Invoke(new List<string>(data));
        data = new List<string>();
        data.Add(fileName);
        data.Add(fileHeaders);
    }

    public override void Subscribe(Action<Vector3, AccelerationTypes> subscriber)
    {
        OnAccelerate += subscriber;
    }

    public override void Unsubscribe(Action<Vector3, AccelerationTypes> subscriber)
    {
        OnAccelerate -= subscriber;
    }

    public override void TriggerVectorListeners(AccelerationTypes type)
    {
        Debug.Log("TriggerVectorListeners...");
        OnAccelerate?.Invoke(angularAcceleration, type);
    }

    // Calculate angular velocity
    private Vector3 CalculateAngularVelocity(Quaternion previous, Quaternion current, float deltaTime)
    {
        if (deltaTime <= 0.0001f) return Vector3.zero;

        Quaternion deltaRotation = current * Quaternion.Inverse(previous);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f; // Normalize angle to [-180, 180]
        return axis * (angle * Mathf.Deg2Rad / deltaTime); // Angular velocity in radians per second
    }
}
