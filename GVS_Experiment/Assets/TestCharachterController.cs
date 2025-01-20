using UnityEngine;

public class TestCharachterController : MonoBehaviour
{
    public float moveSpeed = 5f;    // Movement speed
    public float rotationSpeed = 100f; // Rotation speed

    void Update()
    {
        // --- Movement (WASDQE) ---
        float moveForward = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0); // Forward/Backward
        float moveRight = Input.GetKey(KeyCode.D) ? 1 : (Input.GetKey(KeyCode.A) ? -1 : 0);  // Right/Left
        float moveUp = Input.GetKey(KeyCode.E) ? 1 : (Input.GetKey(KeyCode.Q) ? -1 : 0);     // Up/Down

        Vector3 movement = new Vector3(moveRight, moveUp, moveForward);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.Self);

        // --- Rotation (RTZFGH) ---
        float rotateX = Input.GetKey(KeyCode.R) ? 1 : (Input.GetKey(KeyCode.F) ? -1 : 0);  // Rotate around X-axis
        float rotateY = Input.GetKey(KeyCode.T) ? 1 : (Input.GetKey(KeyCode.G) ? -1 : 0);  // Rotate around Y-axis
        float rotateZ = (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Y)) ? 1 : (Input.GetKey(KeyCode.H) ? -1 : 0);  // Rotate around Z-axis
        bool reset = Input.GetKey(KeyCode.B);

        Vector3 rotation = new Vector3(rotateX, rotateY, rotateZ);
        transform.Rotate(rotation * rotationSpeed * Time.deltaTime, Space.Self);
        if(reset) transform.rotation = Quaternion.identity;
    }
}

