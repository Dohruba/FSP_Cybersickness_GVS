using System;
using UnityEngine;

public class DatasetPreprocessor : MonoBehaviour
{
    private string female = "f";
    private string male = "m";
    [SerializeField] ExperimentManager manager;
    public bool testing = true;
    public float testingTime = 0;
    public float[] ModelInputCSVtoFloat(string csv)
    {
        string[] strings = csv.Split(new char[] { ',', '\n', ' ', '\r' },
                                     StringSplitOptions.RemoveEmptyEntries);

        strings = RemoveTarget(strings);
        strings = ParseGender(strings);

        float[] parsedNumbers = new float[strings.Length];
        for (int i = 0; i < strings.Length; i++)
        {
            parsedNumbers[i] = float.Parse(strings[i]);
        }
        return parsedNumbers;
    }
    public float[] CSVtoFloat(string csv)
    {

        string[] strings = csv.Split(new char[] { ',', '\n', ' ', '\r' },
                                     StringSplitOptions.RemoveEmptyEntries);
        float[] parsedNumbers = new float[strings.Length];
        for (int i = 0; i < strings.Length; i++)
        {
            parsedNumbers[i] = float.Parse(strings[i]);
        }
        return parsedNumbers;
    }

    public string[] ParseGender(string[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            // Still good to check, but now we shouldn't have nulls
            if (string.IsNullOrEmpty(values[i]))
            {
                continue;
            }

            // Optional: Trim whitespace for safety
            string trimmedValue = values[i].Trim();

            if (trimmedValue.Equals(female, StringComparison.OrdinalIgnoreCase))
            {
                values[i] = "0";
                continue;
            }
            if (trimmedValue.Equals(male, StringComparison.OrdinalIgnoreCase))
            {
                values[i] = "1";
                continue;
            }
        }
        return values;
    }

    private string[] RemoveTarget(string[] values)
    {
        string[] processed = new string[values.Length-10];
        int counter = 0;
        for(int i = 0; i < values.Length; i++)
        {
            if ((i + 1) % 11 != 2)
            {
                processed[counter] = values[i];
                counter++;
            }
        }
        return processed;
    }
}
