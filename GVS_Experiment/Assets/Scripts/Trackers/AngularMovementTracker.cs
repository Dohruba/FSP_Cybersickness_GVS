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
    private Vector3 currentAngularVelocity;
    private Vector3 smoothedAngularVelocity;

    [SerializeField]
    private bool isRecording = false;
    public float maxSpeed = 6;

    // Moving average filter variables
    private List<Vector3> velocityHistory = new List<Vector3>();
    public int smoothingWindow = 30; // Adjust this value for stronger or weaker smoothing

    private event Action<Vector3> OnTracked;

    private void Start()
    {
        previousRotation = headTransform.rotation;
    }

    private void FixedUpdate()
    {
        Track();
    }

    // Track angular movement
    public override void Track()
    {
        currentRotation = headTransform.localRotation;
        currentAngularVelocity = CalculateAngularVelocity(previousRotation, currentRotation, Time.deltaTime);

        // Apply moving average filter to angular velocity
        smoothedAngularVelocity = SmoothWithMovingAverage(currentAngularVelocity);

        previousRotation = currentRotation;
    }

    // Public getter for current smoothed angular velocity
    public Vector3 GetAngularVelocity()
    {
        return smoothedAngularVelocity;
    }

    // Public getter for angular speed (magnitude)
    public float GetAngularSpeed()
    {
        return smoothedAngularVelocity.magnitude;
    }

    public override void StopTracking()
    {
        isRecording = false;
    }

    public override bool IsTracking()
    {
        return isRecording;
    }

    public override void StartTracking()
    {
        isRecording = true;
    }

    public override void Subscribe(Action<Vector3> subscriber)
    {
        Debug.Log("Subscribing...");
        OnTracked += subscriber;
    }

    public override void Unsubscribe(Action<Vector3> subscriber)
    {
        OnTracked -= subscriber;
    }

    public override void TriggerStringListeners()
    {
        Debug.Log("TriggerListeners...");
        OnTracked?.Invoke(smoothedAngularVelocity);
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

    // Moving average filter for angular velocity
    private Vector3 SmoothWithMovingAverage(Vector3 currentVelocity)
    {
        velocityHistory.Add(currentVelocity);

        // Maintain the history size
        if (velocityHistory.Count > smoothingWindow)
        {
            velocityHistory.RemoveAt(0);
        }

        // Calculate the moving average
        Vector3 smoothedVelocity = Vector3.zero;
        foreach (Vector3 velocity in velocityHistory)
        {
            smoothedVelocity += velocity;
        }

        return smoothedVelocity / velocityHistory.Count;
    }
}