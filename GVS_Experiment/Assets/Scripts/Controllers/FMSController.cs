using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Burst.Intrinsics.X86;

public class FMSController : MonoBehaviour
{
    [SerializeField] private FMSTracker FMSTracker;
    [Header("Input Actions")]
    public InputActionAsset inputActions;
    public InputAction rightJoystickAction;
    public InputAction leftJoystickAction;

    [Header("GVS parameters")]
    [SerializeField]
    private int triggerFms = 5;
    [SerializeField]
    private GVSCDataSender gVSCDataSender;
    [SerializeField]
    private float speed = 1;
    public float currentGvsStrength = 0;


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
        if (FMSTracker.GetCurrentFMS() >= 5 && gVSCDataSender.NoisyGVS.Interpolator < 1)
        {
            currentGvsStrength = gVSCDataSender.NoisyGVS.ActivateNoisyGVS(Time.deltaTime * speed);
        }
        if (FMSTracker.GetCurrentFMS() < 5 && gVSCDataSender.NoisyGVS.Interpolator > 0)
        {
            currentGvsStrength = gVSCDataSender.NoisyGVS.DectivateNoisyGVS(Time.deltaTime * speed);
        }
}
    private void OnRightButtonPressed(InputAction.CallbackContext context)
    {
        FMSTracker.IncreaseFMS();
    }

    private void OnLeftButtonPressed(InputAction.CallbackContext context)
    {
        FMSTracker.DecreaseFMS();
    }
}
