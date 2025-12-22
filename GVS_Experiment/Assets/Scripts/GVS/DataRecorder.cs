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

    [Header("Recording Settings")]
    [SerializeField]
    private float recordingInterval = 0.5f; // Time between recordings in seconds

    private string[] data = new string[11];
    private string filePath;
    private Coroutine recordingCoroutine;
    private string currentEntry = "";

    public string CurrentEntry { get => currentEntry; private set => currentEntry = value; }

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

    public void RecordData(List<string> data)
    {
        string path = Path.Combine(filePath, $"{experimentManager.GenerateFileName()}.csv");
        CurrentEntry = GenerateEntry();
        File.AppendAllText(path, CurrentEntry);
    }

    public void RecordLevelMetrics(string line)
    {
        string filePath = Application.persistentDataPath;
        string uniquePath = Path.Combine(filePath, $"{experimentManager.GenerateFileName()}levels.csv");
        File.AppendAllText(uniquePath, line);
    }

    // Also record realfms vs predicted fms
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
        line = line.TrimEnd(','); // Remove trailing comma
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
        while (true)
        {
            RecordData(null);
            yield return new WaitForSeconds(recordingInterval);
        }
    }
}