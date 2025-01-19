using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementTracker : GVSReporterBase
{
    // Variables for tracking head movement
    public Transform headTransform;
    private Vector3 previousHeadPosition;
    private Vector3 currentHeadPosition;
    private Vector3 currentHeadVelocity;
    private Vector3 previousHeadVelocity;
    private Vector3 headAcceleration;
    private float accelerationValue;
    private float headMovementSpeed;
    private bool isRecording;
    private List<string> data;
    private string fileName = "HeadMovement";
    private string fileHeaders = "id,s,m/s,m/s^2";
    private int batchSize = 1000;
    private AccelerationTypes accType = AccelerationTypes.Linear;
    private string id;

    // Smoothing parameters
    private List<Vector3> velocityHistory = new List<Vector3>();
    private int smoothingWindow = 5; // Choose an appropriate window size for smoothing velocity
    private float accelerationFilter = 0.1f; // Low-pass filter strength for acceleration
    private float filteredAcceleration = 0f;

    private event Action<List<string>> OnTracked;
    private event Action<Vector3, AccelerationTypes> OnAccelerate;

    private void Start()
    {
        id = ExperimentManager.GetGuid();
        data = new List<string>();
        previousHeadPosition = headTransform.position;
        previousHeadVelocity = Vector3.zero;
        data.Add(fileName);
        data.Add(fileHeaders);
    }
    void FixedUpdate()
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
    // Track head linear movement
    public override void Track()
    {
        currentHeadPosition = headTransform.position;
        currentHeadVelocity = (currentHeadPosition - previousHeadPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

        currentHeadVelocity = SmoothVelocity(currentHeadVelocity);

        headMovementSpeed = currentHeadVelocity.magnitude;
        // Calculate acceleration (change in velocity)
        headAcceleration = (currentHeadVelocity - previousHeadVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);
        accelerationValue = (currentHeadVelocity.magnitude - previousHeadVelocity.magnitude) / Mathf.Max(Time.deltaTime, 0.0001f);
        accelerationValue = ApplyLowPassFilter(accelerationValue);

        OnAccelerate?.Invoke(headAcceleration, accType);
        string line = $"{id},{Time.time:F4},{headMovementSpeed:F4},{accelerationValue:F4}";
        data.Add(line);

        previousHeadPosition = currentHeadPosition;
        previousHeadVelocity = currentHeadVelocity;
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
        OnAccelerate?.Invoke(headAcceleration, type);
    }
    // Function to smooth velocity using a moving average
    private Vector3 SmoothVelocity(Vector3 currentVelocity)
    {
        velocityHistory.Add(currentVelocity);
        if (velocityHistory.Count > smoothingWindow)
        {
            velocityHistory.RemoveAt(0);
        }
        return velocityHistory.Aggregate(Vector3.zero, (sum, v) => sum + v) / velocityHistory.Count;
    }

    // Function to apply low-pass filter to acceleration
    private float ApplyLowPassFilter(float rawAcceleration)
    {
        filteredAcceleration = (1 - accelerationFilter) * filteredAcceleration + accelerationFilter * rawAcceleration;
        return filteredAcceleration;
    }

}