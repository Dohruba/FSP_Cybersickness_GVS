using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class ExternalModelCLientNoFMS : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "http://127.0.0.1:5000";
    [SerializeField] private float timeout = 5f;

    [Header("Test Data")]
    [SerializeField] private string sampleCsv;
    [SerializeField] private bool isTesting = false;

    [Header("Debug")]
    [SerializeField] private bool logRequests = true;

    [Header("Manager")]
    [SerializeField] private ExperimentManager experimentManager;

    [Header("Response")]
    [SerializeField] private float predictedFMS = 0;

    private bool serverAvailable = false;


    [System.Serializable]
    public class PredictionRequest
    {
        public string csv_data;
        public float[] features;
    }

    [System.Serializable]
    public class PredictionResponse
    {
        public float prediction;
        public float actual_fms;
        public float error;
        public string status;
        public string message;
        public string test_type;
        public bool is_reasonable;
        public int features_used;
    }

    void Start()
    {
        StartCoroutine(CheckServerHealth());
    }

    // Keyboard controls for testing
    void Update()
    {
        if(isTesting){
            if (Input.GetKeyDown(KeyCode.G))
            {
                RunGoldenTest();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                TestWithSampleCSV();
            }

            if (Input.GetKeyDown(KeyCode.P) && !string.IsNullOrEmpty(sampleCsv))
            {
                PredictFromCSV(sampleCsv);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                StartCoroutine(CheckServerHealth());
            }
        }
    }

    // Prediction options
    public void PredictFromCSV(string csvData)
    {
        if (!serverAvailable)
        {
            Debug.LogError("Server not available. Run model_server.py first.");
            return;
        }
        string processedCsv = csvData
            .Replace(' ', '\n')
            .Replace('f', '1')
            .Replace('m', '1');

        StartCoroutine(PredictFromCSVCoroutine(processedCsv));
    }
    public void PredictFromCSV(string[] csvData)
    {
        if (!serverAvailable)
        {
            Debug.LogError("Server not available. Run model_server.py first.");
            return;
        }
        if (csvData.Length != 10)
        {
            Debug.LogError("Buffer must have 10 entries exactly.");
            return;
        }
        string temp = "";
        for (int i = 0; i < 10; i++)
        {
            temp += csvData[i];
        }
        string processedCsv = temp
            .Replace(' ', '\n')
            .Replace('f', '1')
            .Replace('m', '1');

        StartCoroutine(PredictFromCSVCoroutine(processedCsv));
    }

    private IEnumerator PredictFromCSVCoroutine(string csvData)
    {
        string url = serverUrl + "/predict_from_csv";

        var payload = new PredictionRequest { csv_data = csvData };
        string json = JsonUtility.ToJson(payload);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        if (logRequests)
        {
            Debug.Log("Sending CSV to no-FMS model server...");
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

                if (response.status == "success")
                {
                    predictedFMS = response.prediction;
                    //Debug.Log("No-FMS model prediction: " + response.prediction.ToString("F2"));
                    //Debug.Log("Actual FMS: " + response.actual_fms.ToString("F2"));
                    //Debug.Log("Error: " + response.error.ToString("F2"));
                    //Debug.Log("Features used: " + response.features_used);
                }
                else
                {
                    Debug.LogError("Prediction failed: " + responseText);
                }
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }


    // Testing functionas
    private IEnumerator CheckServerHealth()
    {
        string url = serverUrl + "/health";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = Mathf.RoundToInt(timeout);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                serverAvailable = true;
                Debug.Log("No-FMS model server connected: " + serverUrl);
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                serverAvailable = false;
                Debug.LogError("No-FMS model server not available: " + request.error);
            }
        }
    }

    public void RunGoldenTest()
    {
        StartCoroutine(GoldenTestCoroutine());
    }

    private IEnumerator GoldenTestCoroutine()
    {
        string url = serverUrl + "/golden_test";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = Mathf.RoundToInt(timeout);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<PredictionResponse>(responseText);

                if (response.status == "success")
                {
                    Debug.Log("Golden Test Result:");
                    Debug.Log("Prediction: " + response.prediction.ToString("F2"));
                    Debug.Log("Is reasonable (0-20 range): " + response.is_reasonable);
                    Debug.Log("Message: " + response.message);

                    if (response.is_reasonable)
                    {
                        Debug.Log("Golden test PASSED - model is working correctly!");
                    }
                    else
                    {
                        Debug.LogWarning("Golden test WARNING - prediction outside expected range");
                    }
                }
                else
                {
                    Debug.LogError("Golden test failed: " + responseText);
                }
            }
            else
            {
                Debug.LogError("Golden test request failed: " + request.error);
            }
        }
    }

    public void TestWithSampleCSV()
    {
        StartCoroutine(TestWithSampleCSVCoroutine());
    }

    private IEnumerator TestWithSampleCSVCoroutine()
    {
        string url = serverUrl + "/test_with_sample";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = Mathf.RoundToInt(timeout);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                var response = JsonUtility.FromJson<PredictionResponse>(responseText);

                if (response.status == "success")
                {
                    Debug.Log("Sample CSV Test Result:");
                    Debug.Log("Prediction: " + response.prediction.ToString("F2"));
                    Debug.Log("Actual FMS: " + response.actual_fms.ToString("F2"));
                    Debug.Log("Error: " + response.error.ToString("F2"));
                    Debug.Log("Message: " + response.message);

                    if (response.error < 1.0f)
                    {
                        Debug.Log("Sample test PASSED");
                    }
                    else if (response.error < 3.0f)
                    {
                        Debug.Log("Sample test ACCEPTABLE");
                    }
                    else
                    {
                        Debug.LogWarning("Sample test POOR");
                    }
                }
                else
                {
                    Debug.LogError("Sample test failed: " + responseText);
                }
            }
            else
            {
                Debug.LogError("Sample test request failed: " + request.error);
            }
        }
    }

    public void TestWithProvidedCSV(string csvData)
    {
        PredictFromCSV(csvData);
    }

    public float GetPredictedFMS()
    {
        return predictedFMS;
    }
}