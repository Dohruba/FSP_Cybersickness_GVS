using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public float movementDistance = 4f;
    public float startingHeight;

    public float cycleTime = 4f;

    // Internal variables
    private float initialYPosition;
    private float movementSpeed;

    void Start()
    {
        // Record the initial Y position of the object
        initialYPosition = transform.position.y;

        // Calculate movement speed for a complete cycle
        movementSpeed = Mathf.PI * 2 / cycleTime;
        startingHeight = movementDistance / 2;
        transform.position = new Vector3(transform.position.x, startingHeight, transform.position.z);
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newYPosition = initialYPosition + Mathf.Sin(Time.time * movementSpeed) * (movementDistance / 2f);

        // Update the object's position
        transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);
    }
}
