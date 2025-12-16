using Unity.InferenceEngine;
using UnityEngine;

public class ModelInputProvider : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ModelRunner modelRunner;
    [SerializeField] private float time;
    [SerializeField] private float mov_x;
    [SerializeField] private float mov_y;
    [SerializeField] private float mov_z;
    [SerializeField] private float rot_x;
    [SerializeField] private float rot_y;
    [SerializeField] private float rot_z;
    [SerializeField] private float gender;
    [SerializeField] private float MSSQ;
    [SerializeField] private float age;

    // Example: Simple test data
    private float[] testInput = new float[10]
    {
        206.5254f,  // time
        0.061f,  // Movement X
        0f,  // Movement Y
        0.0f,  // Movement Z
        0.05f, // Rotation X
        0.0f,  // Rotation Y
        0.0f,  // Rotation Z
        1f,    // Example: Gender encoding (0=female, 1=male, 0.5=other)
        0.15f, // MSSQ
        0.05f  // Age
    };

    void Start()
    {
        // Safety check
        if (modelRunner == null)
        {
            Debug.LogError("Assign ModelRunner in the Inspector");
        }
    }

    void Update()
    {
        // Optional: Run on key press for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunModelWithCurrentData();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            testInput = new float[10]
            {
                time,
                mov_x, mov_y, mov_z,
                rot_x, rot_y, rot_z,
                gender,
                MSSQ,
                age
            };
        }
    }

    public void RunModelWithCurrentData()
    {
        if (modelRunner == null) return;

        // Prepare your actual data here
        // For now, using test data
        float[] currentInput = PrepareInputData();

        // Send to model
        Tensor<float> tensorInput = new Tensor<float>(new TensorShape(1,10), currentInput, 0);
        modelRunner.RunModel(tensorInput);
    }

    float[] PrepareInputData()
    {
        // REPLACE THIS WITH YOUR ACTUAL DATA COLLECTION
        // This is where you gather data from your game

        // Example: Get data from various sources
        // float genderCode = GetGenderEncoding();
        // Vector3 movement = GetMovementData();
        // etc...

        // For testing, return the test data
        return (float[])testInput.Clone();
    }

    // ADD YOUR ACTUAL DATA METHODS HERE:
    // Example methods you'll need to implement:
    /*
    float GetGenderEncoding()
    {
        // Return 0.0f, 1.0f, or 0.5f based on user data
    }
    
    Vector3 GetMovement()
    {
        // Return character/object movement vector
    }
    
    Vector3 GetAngularMovement()
    {
        // Return rotation data
    }
    */
}