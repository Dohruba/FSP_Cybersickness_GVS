using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementTracker : TrackerBase
{
    // Variables for tracking head movement
    public Transform headTransform;
    private Vector3 previousHeadPosition;
    private Vector3 currentHeadPosition;
    private Vector3 currentHeadVelocity;
    private Vector3 previousHeadVelocity;
    private Vector3 headAcceleration;
    private float headMovementSpeed;
    private bool isRecording;
    private List<string> data;
    private string fileName = "HeadMovement";
    private int batchSize = 1000;

    private event Action<List<string>> OnTracked;

    private void Start()
    {
        data = new List<string>();
        previousHeadPosition = headTransform.position;
        previousHeadVelocity = Vector3.zero;
        data.Add(fileName);

    }
    void FixedUpdate()
    {
        if (isRecording)
        {
            Track();
            if (data.Count > batchSize) // +1 for the fileName
            {
                Trigger();
            }
        }
    }
    // Track head linear movement
    public override void Track()
    {
        currentHeadPosition = headTransform.position;
        currentHeadVelocity = (currentHeadPosition - previousHeadPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        headMovementSpeed = currentHeadVelocity.magnitude;
        // Calculate acceleration (change in velocity)
        headAcceleration = (currentHeadVelocity - previousHeadVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);

        string line = $"{Time.time:F4},{headMovementSpeed:F4},{headAcceleration.magnitude:F4}";
        data.Add(line);

        previousHeadPosition = currentHeadPosition;
        previousHeadVelocity = currentHeadVelocity;
    }

    public override void StopRecording()
    {
        // Ensure any remaining data is saved
        if (data.Count > 1) 
        {
            Trigger();
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

    public override void Trigger()
    {
        Debug.Log("Trigger...");
        OnTracked?.Invoke(new List<string>(data));
        data = new List<string>();
        data.Add(fileName);
    }
}