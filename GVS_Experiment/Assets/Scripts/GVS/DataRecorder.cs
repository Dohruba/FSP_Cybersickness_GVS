using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataRecorder : MonoBehaviour
{
    [SerializeField]
    private TrackerBase[] trackers;
    private string filePath;

    private void Awake()
    {
        filePath = Application.persistentDataPath;
        SubscribeToTrackers();
    }

    private void SubscribeToTrackers()
    {
        foreach (var tracker in trackers)
        {
            if (tracker != null)
            {
                tracker.Subscribe(RecordData);
            }
        }
    }

    private void RecordData(List<string> data)
    {
        Debug.Log("Recording");
        if (data == null || data.Count < 3) return;
        Debug.Log("Valid data");
        string origin = data[0];
        string path = Path.Combine(filePath, $"{origin}.csv");
        string uniquePath = Path.Combine(filePath, $"{ExperimentManager.GetGuid()}{origin}.csv");
        // Check if headers are already written to the file
        EnsureHeadersExist(path, data[1]);
        EnsureHeadersExist(uniquePath, data[1]);
        int startIndex = 2;

        string content = string.Join("\n", data.GetRange(startIndex, data.Count - startIndex)) + "\n";

        // Append the data to the CSV file
        File.AppendAllText(path, content);
        File.AppendAllText(uniquePath, content);
        Debug.Log($"Data recorded to {path}");
        Debug.Log($"Data recorded to {uniquePath}");
    }
    public static void RecordLevelMetrics(string line)
    {
        string filePath = Application.persistentDataPath;
        string uniquePath = Path.Combine(filePath, $"{ExperimentManager.GetGuid()}levels.csv");
        File.AppendAllText(uniquePath, line);
    }
    private void EnsureHeadersExist(string path, string headers)
    {
        if (!File.Exists(path))
        {
            // File doesn't exist, so create it with the headers
            File.WriteAllText(path, headers + "\n");

        }
        else
        {
            // Check if the file already contains headers
            string firstLine = File.ReadLines(path).First();
            if (firstLine.Trim() != headers.Trim())
            {
                // If headers are missing, prepend them
                string existingContent = File.ReadAllText(path);
                File.WriteAllText(path, headers + "\n" + existingContent);
            }
        }
    }
}
