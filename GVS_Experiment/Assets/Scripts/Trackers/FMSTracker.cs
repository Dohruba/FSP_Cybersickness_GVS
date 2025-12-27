using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;


public class FMSTracker : MonoBehaviour
{
    [SerializeField]
    private UIManager uiManager;

    [SerializeField]
    private int fms = 0;
    private int upperLimit = 20;
    private int lowerLimit = 0;
    public int GetCurrentFMS()
    {
        return fms;
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
        if ((change > 0 && fms < upperLimit) || (change < 0 && fms > lowerLimit))
        {
            fms += change;
            if(uiManager != null) 
                uiManager.UpdateFMS(fms);
        }
    }

}