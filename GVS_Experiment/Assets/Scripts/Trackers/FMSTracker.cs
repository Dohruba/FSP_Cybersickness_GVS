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

    public float PredictedFms { get => predictedFms; set => predictedFms = value; }
    public int UserFms { get => userFms; set => userFms = value; }

    public float GetCurrentFMS()
    {
        float userFmsFloat = UserFms;
        return experimentManager.IsGvsUserControlled ? userFmsFloat : PredictedFms;
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
        if ((change > 0 && UserFms < upperLimit) || (change < 0 && UserFms > lowerLimit))
        {
            UserFms += change;
            if(uiManager != null) 
                uiManager.UpdateFMS(UserFms);
        }
    }
    private void Update()
    {
        PredictedFms = experimentCLientNoFMS.GetPredictedFMS();
    }

}