using System.IO;
using UnityEngine;
using UnityEngine.XR;

public class MovementTrackingManager : MonoBehaviour
{

    public Transform headTransform;

    // Variables for angular acceleration calculation
    private Vector3 previousHeadRotation;
    private Vector3 currentHeadRotation;
    private Vector3 previousAngularVelocity;
    private Vector3 currentAngularVelocity;
    private Vector3 angularAcceleration;  // Angular acceleration of the head

    // Moment of inertia for the head (simplified as a point mass)
    public float headMass = 1.0f;
    public float headRadius = 0.2f;

    // File paths for CSV export
    private string filePath;
    private bool isRecording = false;

    void Start()
    {
        previousHeadRotation = headTransform.eulerAngles;
        previousAngularVelocity = Vector3.zero;

        filePath = Application.persistentDataPath + "/headMovementSpeeds.csv";
        if (!File.Exists(filePath))
        {
            string header = "Time (s), Head Speed (m/s)\n";
            File.WriteAllText(filePath, header);
        }
        isRecording = true;
    }

    void FixedUpdate()
    {
        if (isRecording)
        {
            CalculateAngularAcceleration();
            SaveData();
        }
    }



    void CalculateAngularAcceleration()
    {
        // Calculate the change in rotation (angular velocity)
        currentHeadRotation = headTransform.eulerAngles;
        currentAngularVelocity = (currentHeadRotation - previousHeadRotation) / Time.deltaTime;
        // Convert angular velocity to radians per second (Unity uses degrees by default)
        currentAngularVelocity = currentAngularVelocity * Mathf.Deg2Rad;  // Convert degrees to radians
        // Calculate the angular acceleration: α = (Δω) / Δt
        angularAcceleration = (currentAngularVelocity - previousAngularVelocity) / Time.deltaTime;
        // Update the previous rotation and angular velocity
        previousHeadRotation = currentHeadRotation;
        previousAngularVelocity = currentAngularVelocity;
    }

    public void StopRecording()
    {
        isRecording = false;
        Debug.Log("Recording Movement stopped. Data saved to " + filePath);
    }

    public bool IsRecording()
    {
        return isRecording;
    }

    public void StartRecording()
    {
        isRecording = true;
        Debug.Log("Recording Movement started. Data will be saved to " + filePath);
    }

    public void SaveData()
    {
        // Get the current time
        float timeElapsed = Time.time;

        // Prepare the data to write: Time, head speed, and angular acceleration
        //string line = timeElapsed.ToString("F4") + "," + headMovementSpeed.ToString("F4") + "," + angularAcceleration.magnitude.ToString("F4") + "\n";

        // Append the data to the CSV file
        //File.AppendAllText(filePath, line);
    }
}
