using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MovementTracker : GVSReporterBase
{
    // Variables for tracking head movement
    public GameObject xrRig;
    public InputActionAsset inputActions; // The input action asset
    public InputAction leftJoystickAction;
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
    private string fileHeaders = "id,s,m/s,m/s^2,x,y,z";
    private int batchSize = 1000;
    private AccelerationTypes accType = AccelerationTypes.Linear;
    private string id;

    // Smoothing parameters
    private List<Vector3> velocityHistory = new List<Vector3>();
    private int smoothingWindow = 5;
    private float accelerationFilter = 0.1f;
    private float filteredAcceleration = 0f;
    private float maxSpeed;

    private event Action<List<string>> OnTracked;
    private event Action<Vector3, AccelerationTypes> OnAccelerate;

    private void Start()
    {
        id = ExperimentManager.GetGuid();
        data = new List<string>();
        previousHeadPosition = xrRig.transform.position;
        previousHeadVelocity = Vector3.zero;
        data.Add(fileName);
        data.Add(fileHeaders);
        maxSpeed = xrRig.GetComponentInChildren<DynamicMoveProvider>().moveSpeed;
    }
    void OnEnable()
    {
        // Get the action for the right joystick from the input actions asset
        leftJoystickAction = inputActions.FindActionMap("XRI Left Locomotion").FindAction("Move");
        leftJoystickAction.Enable();
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
        Vector2 input = leftJoystickAction.ReadValue<Vector2>();
        float inputX = input.x;
        float inputZ = input.y;

        Vector3 inputVelocity = new Vector3(inputX, 0, inputZ) * maxSpeed;
        Vector3 smoothedVelocity = SmoothVelocity(inputVelocity);
        Vector3 inputAcceleration = (smoothedVelocity - previousHeadVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);
        currentHeadPosition = xrRig.transform.position;
        float yAcceleration = (currentHeadPosition.y - previousHeadPosition.y) / Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 finalAcceleration = new Vector3(inputAcceleration.x, yAcceleration, inputAcceleration.z);

        accelerationValue = ApplyLowPassFilter(finalAcceleration.magnitude);
        if (Mathf.Abs(finalAcceleration.x) > Mathf.Abs(finalAcceleration.z))
        {
            Debug.Log("Side move");
        }
        else
        {
            Debug.Log("Front move");
        }
        // Log the acceleration
        OnAccelerate?.Invoke(finalAcceleration, accType);
        string line = $"{id},{Time.time:F4},{smoothedVelocity.magnitude:F4},{accelerationValue:F4}," +
                      $"{finalAcceleration.x:F4},{finalAcceleration.y:F4},{finalAcceleration.z:F4}";
        data.Add(line);

        // Update state for the next frame
        previousHeadPosition = currentHeadPosition;
        previousHeadVelocity = smoothedVelocity;
    }



    public override void StopRecording()
    {
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