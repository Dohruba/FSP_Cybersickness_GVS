using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataRecorder : MonoBehaviour
{
    [Header("Trackers")]
    [SerializeField]
    private MovementTracker movementTracker;
    [SerializeField]
    private AngularMovementTracker angularMovementTracker;
    [SerializeField]
    private FMSTracker fms_Tracker;

    [Header("Management")]
    [SerializeField]
    private ExperimentManager experimentManager;
    [SerializeField]
    private ExternalModelCLientNoFMS predictionsClient;

    [Header("Recording Settings")]
    [SerializeField]
    private float recordingInterval = 0.5f; // Time between recordings in seconds

    private string[] data = new string[11];
    private string filePath;
    private Coroutine recordingCoroutine;
    private string currentEntry = "";
    private List<string> buffer = new List<string>();

    public string CurrentEntry { get => currentEntry; private set => currentEntry = value; }
    public string[] GetBuffer()
    {
        return buffer.ToArray();
    }

    private void Awake()
    {
        filePath = Application.persistentDataPath;
    }

    private void Start()
    {
        data[8] = experimentManager.Gender;
        data[9] = experimentManager.Mssq;
        data[10] = experimentManager.Age;
    }

    public void RecordMovementFmsData()
    {
        string path = Path.Combine(filePath, $"{experimentManager.GenerateFileName()}.csv");
        CurrentEntry = GenerateEntry();
        File.AppendAllText(path, CurrentEntry);
        buffer.Add(CurrentEntry);
        if (buffer.Count > 10)
            buffer.RemoveAt(0);
        //Debug.Log("Test");
    }

    public void RecordActualVsPredictedFMS()
    {
        string path = Path.Combine(filePath, $"{experimentManager.GenerateFileName()}_fms_measure_vs_prediction.csv");
        string time = experimentManager.GetExperimentTime().ToString();
        string currentFms = fms_Tracker.GetCurrentFMS().ToString();
        string predictedFms = predictionsClient.GetPredictedFMS().ToString();
        string entry = time + "," + currentFms + "," + predictedFms + "\n";
        File.AppendAllText(path, entry);

    }

    public void RecordLevelMetrics(string line)
    {
        string filePath = Application.persistentDataPath;
        string uniquePath = Path.Combine(filePath, $"{experimentManager.GenerateFileName()}_levels.csv");
        File.AppendAllText(uniquePath, line);
    }

    private string GenerateEntry()
    {
        Vector3 acceleration = movementTracker.GetLinearAcceleration();
        Vector3 angularVelocity = angularMovementTracker.GetAngularVelocity();
        data[0] = experimentManager.GetExperimentTime().ToString();
        data[1] = fms_Tracker.GetCurrentFMS().ToString();
        data[2] = acceleration.x.ToString();
        data[3] = acceleration.y.ToString();
        data[4] = acceleration.z.ToString();
        data[5] = angularVelocity.x.ToString();
        data[6] = angularVelocity.y.ToString();
        data[7] = angularVelocity.z.ToString();
        string line = "";
        foreach (string item in data)
        {
            line += item + ",";
        }
        line = line.TrimEnd(',');
        line += "\n";
        return line;
    }

    internal void StartRecording()
    {
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
        }
        recordingCoroutine = StartCoroutine(RecordDataCoroutine());
    }

    internal void StopRecording()
    {
        if (recordingCoroutine != null)
        {
            StopCoroutine(recordingCoroutine);
            recordingCoroutine = null;
        }
    }

    private IEnumerator RecordDataCoroutine()
    {
        while (experimentManager.ExperimentRunning)
        {
            RecordMovementFmsData();
            predictionsClient.PredictFromCSV(GetBuffer());
            RecordActualVsPredictedFMS();
            yield return new WaitForSeconds(recordingInterval);
        }
    }
}