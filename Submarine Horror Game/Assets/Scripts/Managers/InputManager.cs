using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    //Gameplay events
    public Action pingEvent = delegate { };

    public Action<Vector3> moveEvent = delegate { };

    public Action<Vector2> lookEvent = delegate { };

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
        playerInputActions = new PlayerInputActions();
        playerInputActions.Gameplay.Enable();

        // Gameplay actions
        playerInputActions.Gameplay.Ping.performed += OnPing;

        playerInputActions.Gameplay.Movement.started += OnMove;
        playerInputActions.Gameplay.Movement.performed += OnMove;
        playerInputActions.Gameplay.Movement.canceled += OnMove;

        playerInputActions.Gameplay.Look.performed += OnLook;
    }

    private void OnDisable()
    {
        // Gameplay actions
        playerInputActions.Gameplay.Ping.performed -= OnPing;

        playerInputActions.Gameplay.Movement.started -= OnMove;
        playerInputActions.Gameplay.Movement.performed -= OnMove;
        playerInputActions.Gameplay.Movement.canceled -= OnMove;

        playerInputActions.Gameplay.Look.performed -= OnLook;
    }

    // Gameplay functions
    private void OnPing(InputAction.CallbackContext context) => pingEvent.Invoke();

    private void OnMove(InputAction.CallbackContext context) => moveEvent.Invoke(context.ReadValue<Vector3>());

    private void OnLook(InputAction.CallbackContext context) => lookEvent.Invoke(context.ReadValue<Vector2>());
}
