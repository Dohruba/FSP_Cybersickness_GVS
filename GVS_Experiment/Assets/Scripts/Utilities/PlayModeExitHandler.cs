using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeExitHandler
{

    private const string EXPERIMENT_COUNTER_KEY = "ExperimentCounter";
    private const string CURRENT_EXPERIMENT_ID_KEY = "CurrentExperimentID";
    static PlayModeExitHandler()
    {
        // Subscribe to the play mode state change event
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        if (!EditorPrefs.HasKey(EXPERIMENT_COUNTER_KEY))
        {
            EditorPrefs.SetInt(EXPERIMENT_COUNTER_KEY, 0);
        }
        Debug.Log($"Experiment ID Manager initialized. Total experiments: {GetTotalExperimentCount()}");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Exiting Play Mode");
            OnExitEndExperiment();
        }

        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            IncrementExperimentCounter();
            LogCurrentExperimentInfo();
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SetCurrentExperimentID();
        }
    }

    private static void IncrementExperimentCounter()
    {
        int currentCount = EditorPrefs.GetInt(EXPERIMENT_COUNTER_KEY, 0);
        currentCount++;
        EditorPrefs.SetInt(EXPERIMENT_COUNTER_KEY, currentCount);

        Debug.Log($"<color=cyan> Experiment Counter Increased: {currentCount}</color>");
    }
    private static void OnExitEndExperiment()
    {
        ExperimentManager manager = Object.FindAnyObjectByType<ExperimentManager>();
        if (manager != null)
        {
            Debug.Log("Experiment manager found. Ending experiment");
            manager.StopExperiment();
        }
    }

    public static int GetTotalExperimentCount()
    {
        return EditorPrefs.GetInt(EXPERIMENT_COUNTER_KEY, 0);
    }
    // Set the current experiment ID when entering play mode
    private static void SetCurrentExperimentID()
    {
        int nextID = EditorPrefs.GetInt(EXPERIMENT_COUNTER_KEY, 0) + 1;
        EditorPrefs.SetString(CURRENT_EXPERIMENT_ID_KEY, $"EXP-{nextID:000}");

        Debug.Log($"<color=green> Starting Experiment: {GetCurrentExperimentID()}</color>");
    }

    // Get the current experiment ID
    public static string GetCurrentExperimentID()
    {
        int nextID = EditorPrefs.GetInt(EXPERIMENT_COUNTER_KEY, 0) + 1;
        return $"EXP_{nextID:000}";
    }

    private static void LogCurrentExperimentInfo()
    {
        int totalExperiments = GetTotalExperimentCount();
        string lastExperiment = GetLastExperimentID();

        Debug.Log($"<color=magenta>   Total Experiments Completed: {totalExperiments}</color>");
        Debug.Log($"<color=magenta>   Last Experiment ID: {lastExperiment}</color>");
        Debug.Log($"<color=magenta>   Next Experiment ID: {GetCurrentExperimentID()}</color>");

        // Save session info to a file for persistent records
        SaveSessionInfo(totalExperiments, lastExperiment);
    }

    // Save detailed session info to a text file
    private static void SaveSessionInfo(int experimentNumber, string experimentID)
    {
        string directoryPath = "Assets/ExperimentLogs/";
        string filePath = directoryPath + "ExperimentHistory.txt";

        // Create directory if it doesn't exist
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        // Create or append to the history file
        string logEntry = $"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                         $"Experiment #{experimentNumber}: {experimentID}\n";

        System.IO.File.AppendAllText(filePath, logEntry);

        Debug.Log($"<color=blue> Experiment logged to: {filePath}</color>");
    }
    public static string GetLastExperimentID()
    {
        int lastID = EditorPrefs.GetInt(EXPERIMENT_COUNTER_KEY, 0);
        return lastID > 0 ? $"EXP_{lastID:000}" : "No experiments yet";
    }
}