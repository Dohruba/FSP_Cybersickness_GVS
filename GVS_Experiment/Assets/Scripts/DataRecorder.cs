using System.Collections.Generic;
using System.IO;
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
        if (data == null || data.Count < 2) return;
        Debug.Log("Valid data");

        string origin = data[0];
        string path = Path.Combine(filePath, $"{origin}.csv");
        string content = string.Join("\n", data.GetRange(1, data.Count - 1)) + "\n";
        // Append the data to the CSV file
        File.AppendAllText(path, content);
        Debug.Log($"Data recorded to {path}");
    }
}
