// Save as: ExternalModelClient.cs
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class ExternalModelClient : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "http://127.0.0.1:5000";
    [SerializeField] private float timeout = 5f;

    [Header("Debug")]
    [SerializeField] private bool logRequests = true;
    [SerializeField] private DatasetPreprocessor datasetPreprocessor;
    [SerializeField] private string sampleCsv;
    [SerializeField] private string fmsCsv; // A CSV file with the FMS history


    private bool serverAvailable = false;
    // Event for prediction results
    public event Action<float> OnPredictionReceived;

    void Start()
    {
        StartCoroutine(CheckServerHealth());
    }



    // Update loop for testing
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestGoldenData();
        }

        if (Input.GetKeyDown(KeyCode.Y) && datasetPreprocessor != null)
        {
            // Convert your CSV to features and predict
            float[] features = datasetPreprocessor.ModelInputCSVtoFloat(sampleCsv);

            // Add temporal features (you'll need to get FMS sequence)
            float[] fmsSequence = datasetPreprocessor.CSVtoFloat(fmsCsv);
            fmsSequence = datasetPreprocessor.ExtractFmsWindow(fmsSequence);
            float[] temporal = CalculateTemporalFeatures(fmsSequence);

            // Combine (107 features total)
            float[] allFeatures = new float[107];
            Array.Copy(features, 0, allFeatures, 0, features.Length);
            Array.Copy(temporal, 0, allFeatures, features.Length, temporal.Length);

            PredictFromFeatures(allFeatures);
        }
    }


    private IEnumerator CheckServerHealth()
    {
        string url = $"{serverUrl}/health";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = Mathf.RoundToInt(timeout);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                serverAvailable = true;
                Debug.Log($"  Model server connected: {serverUrl}");
            }
            else
            {
                serverAvailable = false;
                Debug.LogError($"  Model server not available: {request.error}");
                Debug.LogWarning("Make sure to run: python model_server.py");
            }
        }
    }


    public void PredictFromCSV(string csvData)
    {
        if (!serverAvailable)
        {
            Debug.LogError("Server not available. Run model_server.py first.");
            return;
        }

        StartCoroutine(PredictFromCSVCoroutine(csvData));
    }

    private IEnumerator PredictFromCSVCoroutine(string csvData)
    {
        string url = $"{serverUrl}/predict_from_csv";

        // Prepare JSON payload
        var payload = new PredictionRequestCSV { csv_data = csvData };
        string json = JsonUtility.ToJson(payload);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        if (logRequests)
        {
            Debug.Log($"Sending request to: {url}");
            Debug.Log($"CSV data length: {csvData.Length} chars");
        }

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.timeout = Mathf.RoundToInt(timeout);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<PredictionResponse>(responseText);

                if (logRequests)
                {
                    Debug.Log($"  Prediction: {response.prediction:F2}");
                    if (response.actual_fms > 0)
                    {
                        Debug.Log($"   Actual FMS: {response.actual_fms:F2}");
                        Debug.Log($"   Error: {Mathf.Abs(response.prediction - response.actual_fms):F2}");
                    }
                }

                // You can trigger events or update UI here
                OnPredictionReceived?.Invoke(response.prediction);
            }
            else
            {
                Debug.LogError($"Prediction failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }

    public void PredictFromFeatures(float[] features)
    {
        if (!serverAvailable)
        {
            Debug.LogError("Server not available. Run model_server.py first.");
            return;
        }

        StartCoroutine(PredictFromFeaturesCoroutine(features));
    }

    private IEnumerator PredictFromFeaturesCoroutine(float[] features)
    {
        string url = $"{serverUrl}/predict";

        // Prepare JSON payload
        var payload = new PredictionRequestFeatures { features = features };
        string json = JsonUtility.ToJson(payload);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        if (logRequests)
        {
            Debug.Log($"Sending {features.Length} features to server...");
        }

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.timeout = Mathf.RoundToInt(timeout);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<PredictionResponse>(responseText);

                Debug.Log($"  Server prediction: {response.prediction:F4}");
                OnPredictionReceived?.Invoke(response.prediction);
            }
            else
            {
                Debug.LogError($"Prediction failed: {request.error}");
            }

        }
    }

    public void TestGoldenData()
    {
        StartCoroutine(TestGoldenDataCoroutine());
    }

    private IEnumerator TestGoldenDataCoroutine()
    {
        string url = $"{serverUrl}/test_golden";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = Mathf.RoundToInt(timeout);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<PredictionResponse>(responseText);

                Debug.Log($"   Golden Test Result:");
                Debug.Log($"   Prediction: {response.prediction:F4}");
                Debug.Log($"   Expected: 13.0");
                Debug.Log($"   Error: {response.error:F4}");

                if (Mathf.Abs(response.prediction - 13.0f) < 0.5f)
                {
                    Debug.Log("  Server is working correctly!");
                }
                else
                {
                    Debug.LogWarning("  Server prediction doesn't match expected");
                }
            }
            else
            {
                Debug.LogError($"Test failed: {request.error}");
            }
        }
    }

    private float[] CalculateTemporalFeatures(float[] fmsSequence)
    {
        float[] temporal = new float[7];
        int n = fmsSequence.Length;

        // Feature 0: First FMS in window
        temporal[0] = fmsSequence[0];

        // Feature 1: Mean FMS
        float sum = 0f;
        for (int i = 0; i < n; i++)
        {
            sum += fmsSequence[i];
        }
        float mean = sum / n;
        temporal[1] = mean;

        // Feature 2: Standard deviation of FMS
        float variance = 0f;
        for (int i = 0; i < n; i++)
        {
            float diff = fmsSequence[i] - mean;
            variance += diff * diff;
        }
        float std = Mathf.Sqrt(variance / n);
        temporal[2] = std;

        // Feature 3: Range (max - min)
        float min = fmsSequence[0];
        float max = fmsSequence[0];
        for (int i = 1; i < n; i++)
        {
            if (fmsSequence[i] < min) min = fmsSequence[i];
            if (fmsSequence[i] > max) max = fmsSequence[i];
        }
        temporal[3] = max - min;

        // Feature 4: Total change (last - first)
        temporal[4] = fmsSequence[n - 1] - fmsSequence[0];

        // Feature 5 & 6: Velocity features (rate of change)
        if (n > 1)
        {
            float[] velocities = new float[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                velocities[i] = fmsSequence[i + 1] - fmsSequence[i];
            }

            // Feature 5: Average velocity (mean of differences)
            float velocitySum = 0f;
            for (int i = 0; i < velocities.Length; i++)
            {
                velocitySum += velocities[i];
            }
            float velocityMean = velocitySum / velocities.Length;
            temporal[5] = velocityMean;

            // Feature 6: Velocity variability (std of differences)
            float velocityVariance = 0f;
            for (int i = 0; i < velocities.Length; i++)
            {
                float diff = velocities[i] - velocityMean;
                velocityVariance += diff * diff;
            }
            float velocityStd = Mathf.Sqrt(velocityVariance / velocities.Length);
            temporal[6] = velocityStd;
        }
        else
        {
            temporal[5] = 0f;
            temporal[6] = 0f;
        }

        return temporal;
    }

}

// JSON serialization classes
[System.Serializable]
public class PredictionRequestCSV
{
    public string csv_data;
}

[System.Serializable]
public class PredictionRequestFeatures
{
    public float[] features;
}

[System.Serializable]
public class PredictionResponse
{
    public float prediction;
    public float actual_fms;
    public float error;
    public string status;
    public int features_used;
}