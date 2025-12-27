using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GVS;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;


public class FMSTracker : MonoBehaviour
{
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private ExperimentManager experimentManager;
    [SerializeField]
    private ExternalModelCLientNoFMS experimentCLientNoFMS;

    [SerializeField]
    private int userFms = 0;
    [SerializeField]
    private float predictedFms = 0;
    private int upperLimit = 20;
    private int lowerLimit = 0;


    public float GetCurrentFMS()
    {
        float userFmsFloat = userFms;
        return experimentManager.IsGvsUserControlled ? userFmsFloat : predictedFms;
    }
    public void IncreaseFMS()
    {
        UpdateFMS(1);
    }
    public void DecreaseFMS()
    {
        UpdateFMS(-1);
    }

    private void UpdateFMS(int change)
    {
        if ((change > 0 && userFms < upperLimit) || (change < 0 && userFms > lowerLimit))
        {
            userFms += change;
            if(uiManager != null) 
                uiManager.UpdateFMS(userFms);
        }
    }
    private void Update()
    {
        predictedFms = experimentCLientNoFMS.GetPredictedFMS();
    }

}