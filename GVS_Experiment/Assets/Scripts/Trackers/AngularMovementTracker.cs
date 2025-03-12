using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AngularMovementTracker : TrackerBase
{
    // Variables for tracking angular movement
    public Transform headTransform;
    private Quaternion previousRotation;
    private Quaternion currentRotation;
    private Vector3 previousAngularVelocity;
    private Vector3 currentAngularVelocity;
    private Vector3 angularAcceleration;
    private Vector3 smoothedAngularAcceleration;
    private float angularSpeed;
    [SerializeField]
    private bool isRecording = false;
    private List<string> data;
    private string fileName = "AngularMovement";
    private string fileHeaders = "id,s,deg/s,deg/s^2,x,y,z";
    private int batchSize = 1000;
    private string id;
    public float maxSpeed = 6;
    public float maxAcc = 60;

    // Moving average filter variables
    private List<Vector3> accelerationHistory = new List<Vector3>();
    public int smoothingWindow = 30; // Adjust this value for stronger or weaker smoothing

    private event Action<List<string>> OnTracked;

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
        currentRotation = headTransform.localRotation;
        currentAngularVelocity = CalculateAngularVelocity(previousRotation, currentRotation, Time.deltaTime);
        angularSpeed = currentAngularVelocity.magnitude;
        if(angularSpeed > maxSpeed) return; // Block excessively big values
        angularAcceleration = (currentAngularVelocity - previousAngularVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);
        // Apply moving average filter to angular acceleration
        smoothedAngularAcceleration = SmoothWithMovingAverage(angularAcceleration);
        if(Mathf.Abs(smoothedAngularAcceleration.y) > maxAcc) return;

        string line = $"{id},{Time.time:F4},{angularSpeed:F4},{smoothedAngularAcceleration.magnitude:F4}" +
                  $",{smoothedAngularAcceleration.x:F4},{smoothedAngularAcceleration.y:F4},{smoothedAngularAcceleration.z:F4}";
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

    // Calculate angular velocity
    private Vector3 CalculateAngularVelocity(Quaternion previous, Quaternion current, float deltaTime)
    {
        if (deltaTime <= 0.0001f) return Vector3.zero;

        Quaternion deltaRotation = current * Quaternion.Inverse(previous);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;
        return axis * (angle * Mathf.Deg2Rad / deltaTime);
    }

    // Moving average filter for angular acceleration
    private Vector3 SmoothWithMovingAverage(Vector3 currentAcceleration)
    {
        accelerationHistory.Add(currentAcceleration);

        // Maintain the history size
        if (accelerationHistory.Count > smoothingWindow)
        {
            accelerationHistory.RemoveAt(0);
        }

        // Calculate the moving average
        Vector3 smoothedAcceleration = Vector3.zero;
        foreach (Vector3 acceleration in accelerationHistory)
        {
            smoothedAcceleration += acceleration;
        }

        return smoothedAcceleration / accelerationHistory.Count;
    }
}
