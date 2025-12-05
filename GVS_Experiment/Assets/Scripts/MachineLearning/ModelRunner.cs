using UnityEngine;
using Unity.InferenceEngine;
using System;
using System.Collections.Generic;

public class ModelRunner : MonoBehaviour
{
    [SerializeField]
    private ModelAsset modelAsset;
    private Worker m_Worker;
    private bool m_ModelReady = false;
    private event Action<float> OnPreductionCompleted;

    void OnEnable()
    {
        var model = ModelLoader.Load(modelAsset);
        m_Worker = new Worker(model, BackendType.CPU);
        m_ModelReady = true;
    }
    void OnDisable()
    {
        // Clean up Inference Engine resources.
        m_Worker.Dispose();
        m_ModelReady = false;
    }

    public void RegisterOnPredictionCompleted(Action<float> callback)
    {
        OnPreductionCompleted += callback;
    }
    public void UnregisterOnPredictionCompleted(Action<float> callback)
    {
        OnPreductionCompleted -= callback;
    }

    public void RunModel(Tensor<float> m_Input)
    {
        if (!m_ModelReady)
        {
            Debug.LogError("Model is not ready. Ensure OnEnable has completed.");
            return;
        }
        // model has a single input, so no ambiguity due to its name
        m_Worker.Schedule(m_Input);

        // model has a single output, so no ambiguity due to its name
        var outputTensor = m_Worker.PeekOutput() as Tensor<float>;

        // If you wish to read from the tensor, download it to cpu.
        var cpuTensor = outputTensor.ReadbackAndClone();

        // Log the output values
        var sb = new System.Text.StringBuilder();
        sb.Append("Output: [");
        sb.Append(cpuTensor[0]);
        sb.Append("]");
        Debug.Log(sb.ToString());

        cpuTensor.Dispose();
    }

}
