using System;
using System.Text;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Random = System.Random;

public class ExperimentManager : MonoBehaviour
{
    [Header("Experiment Information")]
    [SerializeField]
    private string experimentId = string.Empty;
    [SerializeField]
    private string experimentDate = string.Empty;
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
    private bool isGvsUserControlled = true;
    [SerializeField] 
    private GVSCDataSender GVSCDataSender;
    [SerializeField]
    private bool isTesting = false;
    [SerializeField]
    private bool isManual = true;
    [SerializeField]
    private bool logEvents = false;
    [SerializeField]
    private float gvsMiliAmpere = 0.0f;

    [Header("Character controls")]
    [SerializeField]
    private float speed = 0.0f;
    [SerializeField]
    private float rotSpeed = 0.0f;
    [SerializeField]
    private DynamicMoveProvider dynamicMoveProvider;
    [SerializeField]
    private SmoothRotation smoothRotation;

    [Header("FMS values")]
    [SerializeField]
    private float actualFMS = 0;
    [SerializeField]
    private float predictedFMS = 0;
    [SerializeField]
    private FMSTracker fmsTracker;

    // Getters
    public string Gender { get => gender; set => gender = value; }
    public string Age { get => age; set => age = value; }
    public string Mssq { get => mssq; set => mssq = value; }
    public string Identificator { get => identificator; set => identificator = value; }
    public float PredictedFms { get => predictedFms; set => predictedFms = value; }
    public bool ExperimentRunning { get => experimentRunning; set => experimentRunning = value; }
    public bool IsGvsUserControlled { get => isGvsUserControlled; set => isGvsUserControlled = value; }
    public bool IsManual { get => isManual; set => isManual = value; }

    private void Awake()
    {
        gender = gender == "f" ? "0" : "1";
        experimentDate = DateTime.Now.ToShortDateString().Replace('/','-');
        experimentId = PlayModeExitHandler.GetCurrentExperimentID();
        Identificator = GenerateFileName();
        Debug.Log(Identificator.ToString());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StopExperiment();
            Identificator = GenerateFileName();
        }
        GVSCDataSender.IsTesting = isTesting;
        GVSCDataSender.IsManual = IsManual;
        GVSCDataSender.LogEvents = logEvents;
        GVSCDataSender.MaxMiliAmpere = gvsMiliAmpere < 2.5f ? gvsMiliAmpere : 2.5f;
        dynamicMoveProvider.moveSpeed = speed;
        smoothRotation.rotationSpeed = rotSpeed;
        actualFMS = fmsTracker.UserFms;
        predictedFms = fmsTracker.PredictedFms;

    }
    public void StartExperiment()
    {
        if (ExperimentRunning)
        {
            experimentId = PlayModeExitHandler.GetCurrentExperimentID();
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
