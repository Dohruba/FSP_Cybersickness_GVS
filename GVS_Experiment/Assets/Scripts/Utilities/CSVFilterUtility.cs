using System;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public static class CSVFilterUtility
{
    public static float[] LowPassFilter(float[] data, float cutoff, float fs, int order = 5)
    {
        float nyquist = 0.5f * fs;
        float normalCutoff = cutoff / nyquist;

        var (b, a) = ButterworthCoefficients(order, normalCutoff);

        float[] filteredData = new float[data.Length];
        float[] x = new float[order];
        float[] y = new float[order]; 

        for (int i = 0; i < data.Length; i++)
        {
            for (int j = order - 1; j > 0; j--)
            {
                x[j] = x[j - 1];
                y[j] = y[j - 1];
            }
            x[0] = data[i];
            y[0] = b[0] * x[0] + b[1] * x[1] + b[2] * x[2] + b[3] * x[3] + b[4] * x[4]
                   - a[1] * y[1] - a[2] * y[2] - a[3] * y[3] - a[4] * y[4];
            filteredData[i] = y[0];
        }

        return filteredData;
    }

    // Generate Butterworth filter coefficients for a given order and normalized cutoff frequency
    private static (float[], float[]) ButterworthCoefficients(int order, float cutoff)
    {
        float[] b = new float[5];
        float[] a = new float[5];
        b[0] = 0.1f;
        b[1] = 0.1f;
        b[2] = 0.1f;
        b[3] = 0.1f;
        b[4] = 0.1f;

        a[0] = 1.0f;
        a[1] = -0.8f;
        a[2] = 0.6f;
        a[3] = -0.4f;
        a[4] = 0.2f;

        return (b, a);
    }

    public static float[] ClampValues(float[] data, float min, float max)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ClampValue(data[i], min, max);
        }

        return data;
    }

    public static float ClampValue(float value, float min, float max)
    {
        return Math.Clamp(value, min, max);
    }
    public static Vector3 ClampValues(Vector3 data, float min, float max)
    {
        data = new Vector3(
            Math.Clamp(data.x, min, max), Math.Clamp(data.y, min, max), Math.Clamp(data.z, min, max)
            );

        return data;
    }
}
