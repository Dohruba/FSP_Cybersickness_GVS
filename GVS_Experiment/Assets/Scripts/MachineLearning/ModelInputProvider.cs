using System;
using Unity.InferenceEngine;
using UnityEngine;

public class ModelInputProvider : MonoBehaviour
{
    float[] modelInput = new float[10];
    [SerializeField]
    private ModelRunner modelRunner;
    private float timestamp;

    private void Start() {
        //0. Set Time
        //1. Register to movement providers
        //2. Get user data encoded (gender,mssq, age)
        timestamp = Time.realtimeSinceStartup;
    }
    private void FixedUpdate()
    {
        timestamp = Time.realtimeSinceStartup;
        Debug.Log("Timestamp updated: " + timestamp);
    }

    /*
    0. Timestamp
    1. Acc X
    2. Acc Y
    3. Acc Z
    4. Angular Vel X
    5. Angular Vel Y
    6. Angular Vel Z
    7. Gender
    8. Mssq
    9. Age
    */

    void PrepareModelInput()
    {
        //1. Output array
        float[] tempModelInput = new float[10];
        tempModelInput[0] = timestamp;

        //2. Get Gender
        //3. Encode gender
        //4. Get movement info
        //5. Process mov. info
        //6. Prepare input array
    }

    public void PassInputToModel()
    {
        // Call model Runner
    }

}
