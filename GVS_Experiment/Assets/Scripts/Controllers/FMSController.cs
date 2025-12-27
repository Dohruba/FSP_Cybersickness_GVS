using UnityEngine;
using UnityEngine.InputSystem;

public class FMSController : MonoBehaviour
{
    [SerializeField] private FMSTracker FMSTracker;
    [Header("Input Actions")]
    public InputActionAsset inputActions;
    public InputAction rightJoystickAction;
    public InputAction leftJoystickAction;

    void OnEnable()
    {
        rightJoystickAction = inputActions.FindActionMap("XRI Right Interaction").FindAction("FMS");
        leftJoystickAction = inputActions.FindActionMap("XRI Left Interaction").FindAction("FMS");
        rightJoystickAction.Enable();
        leftJoystickAction.Enable();
        rightJoystickAction.performed += OnRightButtonPressed;
        leftJoystickAction.performed += OnLeftButtonPressed;

    }

    void OnDisable()
    {
        rightJoystickAction.performed -= OnRightButtonPressed;
        leftJoystickAction.performed -= OnLeftButtonPressed;
        rightJoystickAction.Disable();
        leftJoystickAction.Disable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            FMSTracker.IncreaseFMS();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            FMSTracker.DecreaseFMS();
        }
    }
    private void OnRightButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("A Button Pressed!");
    }

    private void OnLeftButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("X Button Pressed!");
    }
}
