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
    private Vector3 previousHeadVelocity;
    private Vector3 headAcceleration;
    private float accelerationValue;
    private bool isRecording;
    private List<string> data;
    private string fileName = "HeadMovement";
    private string fileHeaders = "id,s,m/s,m/s^2,x,y,z";
    private int batchSize = 1000;
    private AccelerationTypes accType = AccelerationTypes.Linear;
    private string id;
    private float clamp = 20;

    // Smoothing parameters
    private List<Vector3> velocityHistory = new List<Vector3>();
    private int smoothingWindow = 5;
    private float maxSpeed;

    private float filteredAccelerationX = 0f;
    private float filteredAccelerationY = 0f;
    private float filteredAccelerationZ = 0f;
    private float[] previousFilteredValuesX; // Array to hold previous filtered values
    private float[] previousFilteredValuesY; // Array to hold previous filtered values
    private float[] previousFilteredValuesZ; // Array to hold previous filtered values
    public int filterOrder = 5; // Number of previous values to consider (make it larger for more aggressiveness)
    public float accelerationFilter = 0.9f; // Filter smoothing factor, between 0 and 1
    private int currentIndex = 0; // Keeps track of the index to overwrite
    private Vector3 filteredAcceleration;
    private float previousVelocityY;

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

        previousFilteredValuesX = new float[filterOrder];
        previousFilteredValuesY = new float[filterOrder];
        previousFilteredValuesZ = new float[filterOrder];
        Array.Fill(previousFilteredValuesX, 0f); // Initialize to 0
        Array.Fill(previousFilteredValuesY, 0f); // Initialize to 0
        Array.Fill(previousFilteredValuesZ, 0f); // Initialize to 0
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
        Vector3 inputVelocity = CalculateInputVelocity();
        Vector2 input = leftJoystickAction.ReadValue<Vector2>();
        Vector3 forGVS = new Vector3(input.x, 0, input.y);
        OnAccelerate?.Invoke(forGVS, accType);
        Vector3 smoothedVelocity = SmoothVelocity(inputVelocity);

        Vector3 inputAcceleration = CalculateHorizontalAcceleration(inputVelocity);
        float yAcceleration = CalculateVerticalAcceleration();

        Vector3 gvsAcceleration = new Vector3(inputAcceleration.x, yAcceleration, inputAcceleration.z);
        //OnAccelerate?.Invoke(ApplyLowPassFilter(gvsAcceleration), accType);
        //OnAccelerate?.Invoke(gvsAcceleration, accType);
        Vector3 finalAcceleration = gvsAcceleration;
            //FilterAndClampAcceleration(gvsAcceleration);

        // Log acceleration data
        LogAccelerationData(smoothedVelocity, finalAcceleration);

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
        currentHeadPosition = xrRig.transform.position;
        float currentVelocityY = (currentHeadPosition.y - previousHeadPosition.y) / Mathf.Max(Time.deltaTime, 0.0001f);
        float yAcceleration = (currentVelocityY - previousVelocityY) / Mathf.Max(Time.deltaTime, 0.0001f);
        previousVelocityY = currentVelocityY;
        return yAcceleration;
    }

    private Vector3 FilterAndClampAcceleration(Vector3 acceleration)
    {
        Vector3 finalAcceleration = new Vector3(
            CSVFilterUtility.ClampValue(acceleration.x, -clamp, clamp),
            CSVFilterUtility.ClampValue(acceleration.y, -clamp, clamp),
            CSVFilterUtility.ClampValue(acceleration.z, -clamp, clamp)
        );
        return ApplyAggressiveLowPassFilter(finalAcceleration);
    }

    private void LogAccelerationData(Vector3 smoothedVelocity, Vector3 finalAcceleration)
    {
        accelerationValue = finalAcceleration.magnitude;
        string line = $"{id},{Time.time:F4},{smoothedVelocity.magnitude:F4},{accelerationValue:F4}," +
                      $"{finalAcceleration.x:F4},{finalAcceleration.y:F4},{finalAcceleration.z:F4}";
        data.Add(line);
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
        if (velocityHistory.Count > smoothingWindow)
        {
            velocityHistory.RemoveAt(0);
        }
        return velocityHistory.Aggregate(Vector3.zero, (sum, v) => sum + v) / velocityHistory.Count;
    }
    public Vector3 ApplyAggressiveLowPassFilter(Vector3 rawAcceleration)
    {
        // Calculate the weighted sum of previous filtered values
        float weightedSumX = 0f;
        float weightedSumY = 0f;
        float weightedSumZ = 0f;
        float weightSumX = 0f;
        float weightSumY = 0f;
        float weightSumZ = 0f;

        // Add previous filtered values with exponentially decreasing weights
        for (int i = 0; i < filterOrder; i++)
        {
            float weight = Mathf.Pow(accelerationFilter, i); // Exponentially decaying weight
            weightedSumX += previousFilteredValuesX[i] * weight;
            weightedSumY += previousFilteredValuesY[i] * weight;
            weightedSumZ += previousFilteredValuesZ[i] * weight;
            weightSumX += weight;
            weightSumY += weight;
            weightSumZ += weight;
        }

        // Add the current raw value with the smallest weight
        weightedSumX += rawAcceleration.x * Mathf.Pow(accelerationFilter, filterOrder);
        weightedSumY += rawAcceleration.y * Mathf.Pow(accelerationFilter, filterOrder);
        weightedSumZ += rawAcceleration.z * Mathf.Pow(accelerationFilter, filterOrder);
        weightSumX += Mathf.Pow(accelerationFilter, filterOrder);
        weightSumY += Mathf.Pow(accelerationFilter, filterOrder);
        weightSumZ += Mathf.Pow(accelerationFilter, filterOrder);

        // Calculate the new filtered value
        float filteredAccelerationX = weightedSumX / weightSumX;
        float filteredAccelerationY = weightedSumY / weightSumY;
        float filteredAccelerationZ = weightedSumZ / weightSumZ;

        // Update the array with the new filtered value
        previousFilteredValuesX[currentIndex] = filteredAccelerationX;
        previousFilteredValuesY[currentIndex] = filteredAccelerationY;
        previousFilteredValuesZ[currentIndex] = filteredAccelerationZ;
        currentIndex = (currentIndex + 1) % filterOrder; // Circular buffer behavior

        return new Vector3(filteredAccelerationX, filteredAccelerationY, filteredAccelerationZ);
    }

    private Vector3 ApplyLowPassFilter(Vector3 rawAcceleration)
    {
        filteredAcceleration = (1 - accelerationFilter) * filteredAcceleration + accelerationFilter * rawAcceleration;
        return filteredAcceleration;
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



}