using System;
using System.Text;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment Information")]
    [SerializeField]
    private string experimentId = string.Empty;
    [SerializeField]
    private string experimentDate = string.Empty;
    [SerializeField]
    private string experimentVersion = string.Empty;
    private string identificator;
    [SerializeField]
    private bool experimentRunning;
    [SerializeField]
    private float startTime = 0f;
    
    [Header("Participant Information")]
    [SerializeField]
    private string gender = string.Empty;
    [SerializeField]
    private string age = string.Empty;
    [SerializeField]
    private string mssq = string.Empty;

    [Header("Data processing")]
    [SerializeField]
    private TrackerBase[] trackers;
    [SerializeField]
    private DataRecorder dataRecorder;

    // Getters
    public string Gender { get => gender; set => gender = value; }
    public string Age { get => age; set => age = value; }
    public string Mssq { get => mssq; set => mssq = value; }
    public string Identificator { get => identificator; set => identificator = value; }

    private void Awake()
    {
        Identificator = GenerateFileName();
        Debug.Log(Identificator.ToString());
        gender = gender == "f" ? "0" : "1";
    }

    private void Update()
    {
        // Start and stop experiment using keyboard keys for testing purposes
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartExperiment();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StopExperiment();
            Identificator = GenerateFileName();
        }
    }
    public void StartExperiment()
    {
        if (experimentRunning)
        {
            Debug.LogWarning("Experiment is already running!");
            return;
        }

        foreach (var tracker in trackers)
        {
            tracker.StartTracking();
        }

        experimentRunning = true;
        Debug.Log("Experiment started!");
        startTime = Time.realtimeSinceStartup;
        dataRecorder.StartRecording();
    }

    public void StopExperiment()
    {
        if (!experimentRunning)
        {
            Debug.LogWarning("No experiment is running!");
            return;
        }

        foreach (var tracker in trackers)
        {
            tracker.StopTracking();
        }

        experimentRunning = false;
        Debug.Log("Experiment stopped!");
        dataRecorder.StopRecording();
    }

    public string GenerateFileName()
    {
        return "V-" + experimentVersion + "_D-" + experimentDate + "_id-" + experimentId;
    }

    public float GetExperimentTime()
    {
        return Time.realtimeSinceStartup - startTime;
    }

}
