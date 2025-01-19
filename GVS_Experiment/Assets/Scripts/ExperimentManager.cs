using System;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class ExperimentManager : MonoBehaviour
{
    [SerializeField]
    private TrackerBase[] trackers;

    [SerializeField]
    private DataRecorder dataRecorder;

    [SerializeField]
    private static string guid;

    private bool experimentRunning;
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private static readonly Random random = new Random();

    private void Awake()
    {
        guid = GenerateShortUUID();
        Debug.Log(guid.ToString());
    }

    private void Update()
    {
        // Start and stop experiment using keyboard keys for testing purposes
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartExperiment();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StopExperiment();
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
            tracker.StartRecording();
        }

        experimentRunning = true;
        Debug.Log("Experiment started!");
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
            tracker.StopRecording();
        }

        experimentRunning = false;
        Debug.Log("Experiment stopped!");
    }

    public static string GetGuid()
    {
        return guid.ToString();
    }
    public static string GenerateShortUUID(int length = 5)
    {
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(Alphabet[random.Next(Alphabet.Length)]);
        }
        return sb.ToString();
    }

}
