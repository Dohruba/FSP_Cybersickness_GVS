using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MovementTracker : GVSReporterBase
{
    // Variables for tracking head movement
    public GameObject target;
    public InputActionAsset inputActions;
    public InputActionAsset testInputActions;
    public InputAction leftJoystickAction;
    private Vector3 previousHeadPosition;
    private Vector3 currentHeadPosition;
    private Vector3 previousHeadVelocity;
    public Vector3 currentAcceleration;
    public Vector3 smoothedAcceleration;

    private bool isTracking;
    private AccelerationTypes accType = AccelerationTypes.Linear;
    private float maxSpeed;

    // Smoothing parameters
    private List<Vector3> velocityHistory = new List<Vector3>();
    private List<Vector3> accelerationHistory = new List<Vector3>();
    public int velocitySmoothingWindow = 5;
    public int accelerationSmoothingWindow = 30;

    private float previousVelocityY;

    private event Action<Vector3> OnTracked;
    private event Action<Vector3, AccelerationTypes> OnAccelerate;

    private void Start()
    {
        previousHeadPosition = target.transform.position;
        previousHeadVelocity = Vector3.zero;
        try
        {
            maxSpeed = target.GetComponentInChildren<DynamicMoveProvider>().moveSpeed;
        }
        catch (Exception ex)
        {
            maxSpeed = 10;
        }
    }

    void OnEnable()
    {
        try
        {
            leftJoystickAction = inputActions.FindActionMap("XRI Left Locomotion").FindAction("Move");
        }
        catch(Exception e) 
        {
            leftJoystickAction = testInputActions.FindActionMap("Keyboard").FindAction("Move");
        }
        leftJoystickAction.Enable();
    }

    void FixedUpdate()
    {
        Track();
    }

    // Track head linear movement
    public override void Track()
    {
        // Calculate input velocity
        Vector3 inputVelocity = CalculateInputVelocity();
        Vector3 smoothedVelocity = SmoothVelocity(inputVelocity);

        // Calculate horizontal acceleration (X and Z)
        Vector3 inputAcceleration = CalculateHorizontalAcceleration(smoothedVelocity);

        // Calculate vertical acceleration (Y)
        float yAcceleration = CalculateVerticalAcceleration();

        // Combine into full acceleration vector
        currentAcceleration = new Vector3(inputAcceleration.x, yAcceleration, inputAcceleration.z);

        // Apply smoothing to acceleration
        smoothedAcceleration = SmoothAcceleration(currentAcceleration);

        // Update previous state for the next frame
        UpdatePreviousState(smoothedVelocity);
    }

    private Vector3 CalculateInputVelocity()
    {
        Vector2 input = leftJoystickAction.ReadValue<Vector2>();
        Vector3 velocity = new Vector3(input.x, 0, input.y) * maxSpeed;
        return velocity;
    }

    private Vector3 CalculateHorizontalAcceleration(Vector3 smoothedVelocity)
    {
        return (smoothedVelocity - previousHeadVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    private float CalculateVerticalAcceleration()
    {
        currentHeadPosition = target.transform.position;
        float currentVelocityY = (currentHeadPosition.y - previousHeadPosition.y) / Mathf.Max(Time.deltaTime, 0.0001f);
        float yAcceleration = (currentVelocityY - previousVelocityY) / Mathf.Max(Time.deltaTime, 0.0001f);
        previousVelocityY = currentVelocityY;
        return yAcceleration;
    }
    private void UpdatePreviousState(Vector3 smoothedVelocity)
    {
        previousHeadPosition = currentHeadPosition;
        previousHeadVelocity = smoothedVelocity;
    }

    // Function to smooth velocity using a moving average
    private Vector3 SmoothVelocity(Vector3 currentVelocity)
    {
        velocityHistory.Add(currentVelocity);
        if (velocityHistory.Count > velocitySmoothingWindow)
        {
            velocityHistory.RemoveAt(0);
        }
        return velocityHistory.Aggregate(Vector3.zero, (sum, v) => sum + v) / velocityHistory.Count;
    }

    // Function to smooth acceleration using a moving average
    private Vector3 SmoothAcceleration(Vector3 currentAccel)
    {
        accelerationHistory.Add(currentAccel);
        if (accelerationHistory.Count > accelerationSmoothingWindow)
        {
            accelerationHistory.RemoveAt(0);
        }

        Vector3 smoothed = Vector3.zero;
        foreach (Vector3 accel in accelerationHistory)
        {
            smoothed += accel;
        }

        return smoothed / accelerationHistory.Count;
    }

    // Public getter for current smoothed linear acceleration
    public Vector3 GetLinearAcceleration()
    {
        return smoothedAcceleration;
    }

    // Public getter for acceleration magnitude
    public float GetAccelerationMagnitude()
    {
        return smoothedAcceleration.magnitude;
    }

    public override void StopTracking()
    {
        isTracking = false;
    }

    public override bool IsTracking()
    {
        return isTracking;
    }

    public override void StartTracking()
    {
        isTracking = true;
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
        OnTracked?.Invoke(smoothedAcceleration);
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
        OnAccelerate?.Invoke(smoothedAcceleration, type);
    }

    public void TriggerVectorListeners(Vector3 acceleration)
    {
        OnAccelerate?.Invoke(acceleration, accType);
    }
}