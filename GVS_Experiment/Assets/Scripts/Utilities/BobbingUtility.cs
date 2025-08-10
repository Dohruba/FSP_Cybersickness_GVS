using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class BobbingUtility : MonoBehaviour
{
    [SerializeField] private float walkingBobbingSpeed = 8f;
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private Transform cameraTransform;


    public InputActionAsset inputActions; // The input action asset
    public InputAction leftJoystickAction;

    private float defaultCameraY;
    private float timer = 0f;

    void Start()
    {
        leftJoystickAction = inputActions.FindActionMap("XRI Left Locomotion").FindAction("Move");
        leftJoystickAction.Enable();
        if (cameraTransform != null)
        {
            defaultCameraY = cameraTransform.localPosition.y;
        }
    }

    void Update()
    {
        // Check if the player is moving (using thumbstick input)
        Vector2 leftInput = leftJoystickAction.ReadValue<Vector2>();

        bool isMoving = (leftInput.magnitude > 0.1f);

        // Apply bobbing effect
        if (isMoving && cameraTransform != null)
        {
            timer += Time.deltaTime * walkingBobbingSpeed;
            float waveSlice = Mathf.Sin(timer);
            float verticalOffset = waveSlice * bobbingAmount;
            Vector3 newPosition = cameraTransform.localPosition;
            newPosition.y = defaultCameraY + verticalOffset;
            cameraTransform.localPosition = newPosition;
        }
        else
        {
            // Reset to default position smoothly
            timer = 0f;
            Vector3 newPosition = cameraTransform.localPosition;
            newPosition.y = Mathf.Lerp(newPosition.y, defaultCameraY, Time.deltaTime * walkingBobbingSpeed);
            cameraTransform.localPosition = newPosition;
        }
    }
}
