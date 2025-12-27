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
    private bool isGvsUserControlled = true;
    private string identificator;
    [SerializeField]
    private bool experimentRunning;
    [SerializeField]
    private float startTime = 0f;
    [SerializeField]
    private float predictedFms = 0;
    [SerializeField]
    private string path = "";
    
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

    [Header("GVS Controller")]
    [SerializeField] 
    private GVSCDataSender GVSCDataSender;
    [SerializeField]
    private bool isTesting = false;
    [SerializeField]
    private bool isManual = true;
    [SerializeField]
    private bool logEvents = false;

    // Getters
    public string Gender { get => gender; set => gender = value; }
    public string Age { get => age; set => age = value; }
    public string Mssq { get => mssq; set => mssq = value; }
    public string Identificator { get => identificator; set => identificator = value; }
    public float PredictedFms { get => predictedFms; set => predictedFms = value; }
    public bool ExperimentRunning { get => experimentRunning; set => experimentRunning = value; }
    public bool IsGvsUserControlled { get => isGvsUserControlled; set => isGvsUserControlled = value; }

    private void Awake()
    {
        gender = gender == "f" ? "0" : "1";
        experimentDate = DateTime.Now.ToShortDateString().Replace('/','-');
        Identificator = GenerateFileName();
        Debug.Log(Identificator.ToString());
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
        GVSCDataSender.IsTesting = isTesting;
        GVSCDataSender.IsManual = isManual;
        GVSCDataSender.LogEvents = logEvents;

    }
    public void StartExperiment()
    {
        if (ExperimentRunning)
        {
            Debug.LogWarning("Experiment is already running!");
            return;
        }

        foreach (var tracker in trackers)
        {
            tracker.StartTracking();
        }

        ExperimentRunning = true;
        Debug.Log("Experiment started!");
        startTime = Time.realtimeSinceStartup;
        dataRecorder.StartRecording();
    }

    public void StopExperiment()
    {
        if (!ExperimentRunning)
        {
            Debug.LogWarning("No experiment is running!");
            return;
        }

        foreach (var tracker in trackers)
        {
            tracker.StopTracking();
        }

        ExperimentRunning = false;
        Debug.Log("Experiment stopped!");
        dataRecorder.StopRecording();
    }

    public string GenerateFileName()
    {
        path = "V_" + (IsGvsUserControlled ? "user" : "pred") + " D_" + experimentDate + " id_" + experimentId;
        return path;
    }

    public float GetExperimentTime()
    {
        return Time.realtimeSinceStartup - startTime;
    }


}
