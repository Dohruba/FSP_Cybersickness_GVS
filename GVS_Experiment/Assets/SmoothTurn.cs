using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class SmoothRotation : MonoBehaviour
{
    public GameObject XRRig;
    public InputActionAsset inputActions;
    public InputAction rightJoystickAction;
    public float rotationSpeed = 200f; 
    public float easingFactor = 2;

    void OnEnable()
    {
        rightJoystickAction = inputActions.FindActionMap("XRI Right Locomotion").FindAction("TurnSmooth");
        rightJoystickAction.Enable();
    }

    void OnDisable()
    {
        rightJoystickAction.Disable();
    }

    void Update()
    {
        Vector2 rightJoystickInput = rightJoystickAction.ReadValue<Vector2>();
        float easedInputX = ApplyEasing(rightJoystickInput.x);
        if (easedInputX != 0)
        {
            float rotationAmount = easedInputX * rotationSpeed * Time.deltaTime;
            XRRig.transform.Rotate(Vector3.up, rotationAmount);
        }
    }
    private float ApplyEasing(float input)
    {
        return Mathf.Sign(input) * Mathf.Pow(Mathf.Abs(input), easingFactor);
    }
}
