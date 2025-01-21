using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class SmoothRotation : MonoBehaviour, IGvsReporter
{
    public GameObject XRRig;
    public InputActionAsset inputActions;
    public InputAction rightJoystickAction;
    public float rotationSpeed = 200f; 
    public float easingFactor = 2;
    private AccelerationTypes type = AccelerationTypes.Angular;

    public Action<Vector3, AccelerationTypes> OnTurn { get; private set; }

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
        if(rightJoystickInput != Vector2.zero)
            TriggerVectorListeners(type);
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

    public void Subscribe(Action<Vector3, AccelerationTypes> subscriber)
    {
        OnTurn += subscriber;
    }

    public void Unsubscribe(Action<Vector3, AccelerationTypes> subscriber)
    {
        OnTurn -= subscriber;
    }

    public void TriggerVectorListeners(AccelerationTypes type)
    {
        //Debug.Log("TriggerVectorListeners...");
        Vector2 input = rightJoystickAction.ReadValue<Vector2>();
        Vector3 data = new Vector3(0,input.y,0);
        OnTurn?.Invoke(data, type);
    }
}
