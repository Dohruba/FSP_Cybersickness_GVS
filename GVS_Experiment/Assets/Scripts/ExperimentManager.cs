using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    [SerializeField]
    private TrackerBase[] trackers;

    [SerializeField]
    private DataRecorder dataRecorder;

    private bool experimentRunning;

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


}
