using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    public PlayerInput playerInput;

    public ControlScheme currentControlScheme;

    // Device management
    public Action deviceChangedEvent = delegate { };

    // Gameplay events
    public Action pingEvent = delegate { };

    public Action<Vector3> moveEvent = delegate { };

    public Action<Vector2> lookStickEvent = delegate { };

    public Action<Vector2> lookPositionalEvent = delegate { };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        // Device management
        UpdateControlScheme();
        playerInput.onControlsChanged += OnDeviceChanged;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Gameplay.Enable();

        // Gameplay actions
        playerInputActions.Gameplay.Ping.performed += OnPing;

        playerInputActions.Gameplay.Movement.started += OnMove;
        playerInputActions.Gameplay.Movement.performed += OnMove;
        playerInputActions.Gameplay.Movement.canceled += OnMove;

        playerInputActions.Gameplay.LookStick.started += OnLookStick;
        playerInputActions.Gameplay.LookStick.performed += OnLookStick;
        playerInputActions.Gameplay.LookStick.canceled += OnLookStick;

        playerInputActions.Gameplay.LookPositional.started += OnLookPositional;
        playerInputActions.Gameplay.LookPositional.performed += OnLookPositional;
        playerInputActions.Gameplay.LookPositional.canceled += OnLookPositional;
    }

    private void OnDisable()
    {
        // Device management
        playerInput.onControlsChanged -= OnDeviceChanged;

        // Gameplay actions
        playerInputActions.Gameplay.Ping.performed -= OnPing;

        playerInputActions.Gameplay.Movement.started -= OnMove;
        playerInputActions.Gameplay.Movement.performed -= OnMove;
        playerInputActions.Gameplay.Movement.canceled -= OnMove;

        playerInputActions.Gameplay.LookStick.started -= OnLookStick;
        playerInputActions.Gameplay.LookStick.performed -= OnLookStick;
        playerInputActions.Gameplay.LookStick.canceled -= OnLookStick;

        playerInputActions.Gameplay.LookPositional.started -= OnLookPositional;
        playerInputActions.Gameplay.LookPositional.performed -= OnLookPositional;
        playerInputActions.Gameplay.LookPositional.canceled -= OnLookPositional;
    }

    private void UpdateControlScheme()
    {
        string controlSchemeName = playerInput.currentControlScheme;

        switch (controlSchemeName)
        {
            case "KBM":
                currentControlScheme = ControlScheme.KeyboardAndMouse;
                break;

            case "Gamepad":
                currentControlScheme = ControlScheme.Gamepad;
                break;

            default:
                currentControlScheme = ControlScheme.KeyboardAndMouse;
                break;
        }

        deviceChangedEvent.Invoke();
    }

    // Device management
    private void OnDeviceChanged(PlayerInput _) => UpdateControlScheme();

    // Gameplay functions
    private void OnPing(InputAction.CallbackContext context) => pingEvent.Invoke();

    private void OnMove(InputAction.CallbackContext context) => moveEvent.Invoke(context.ReadValue<Vector3>());

    private void OnLookStick(InputAction.CallbackContext context) => lookStickEvent.Invoke(context.ReadValue<Vector2>());

    private void OnLookPositional(InputAction.CallbackContext context) => lookPositionalEvent.Invoke(context.ReadValue<Vector2>());
}

public enum ControlScheme
{
    KeyboardAndMouse,
    Gamepad
}
