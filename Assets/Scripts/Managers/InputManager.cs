using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	#region Variables

	public static InputManager Instance { get; private set; }

	public PlayerInput PlayerInput;

	// Device management
	public Action DeviceChangedEvent = delegate { };

	// Gameplay events
	public Action PingEvent = delegate { };

	public Action<bool> BeaconHoldEvent = delegate { };

	public Action<Vector3> MoveEvent = delegate { };
	public Action<Vector2> LookEvent = delegate { };

	public Action PauseEvent = delegate { };

	private PlayerInputActions m_playerInputActions;

	#endregion

	#region Unity

	private void Awake()
	{
		if (Instance != null)
			Destroy(this);
		else
			Instance = this;
	}

	private void OnEnable()
	{
		m_playerInputActions = new PlayerInputActions();
		m_playerInputActions.Global.Enable();
		m_playerInputActions.Gameplay.Enable();

		// Global actions
		m_playerInputActions.Global.Pause.performed += OnPause;

		// Gameplay actions
		m_playerInputActions.Gameplay.Ping.performed += OnPing;

		m_playerInputActions.Gameplay.Beacon.started += OnBeaconHold;
		m_playerInputActions.Gameplay.Beacon.performed += OnBeaconHold;
		m_playerInputActions.Gameplay.Beacon.canceled += OnBeaconHold;

		m_playerInputActions.Gameplay.Movement.started += OnMove;
		m_playerInputActions.Gameplay.Movement.performed += OnMove;
		m_playerInputActions.Gameplay.Movement.canceled += OnMove;

		m_playerInputActions.Gameplay.Look.started += OnLook;
		m_playerInputActions.Gameplay.Look.performed += OnLook;
		m_playerInputActions.Gameplay.Look.canceled += OnLook;
	}

	private void OnDisable()
	{
		// Global actions
		m_playerInputActions.Global.Pause.performed -= OnPause;

		// Gameplay actions
		m_playerInputActions.Gameplay.Ping.performed -= OnPing;

		m_playerInputActions.Gameplay.Beacon.started -= OnBeaconHold;
		m_playerInputActions.Gameplay.Beacon.performed -= OnBeaconHold;
		m_playerInputActions.Gameplay.Beacon.canceled -= OnBeaconHold;

		m_playerInputActions.Gameplay.Movement.started -= OnMove;
		m_playerInputActions.Gameplay.Movement.performed -= OnMove;
		m_playerInputActions.Gameplay.Movement.canceled -= OnMove;

		m_playerInputActions.Gameplay.Look.started -= OnLook;
		m_playerInputActions.Gameplay.Look.performed -= OnLook;
		m_playerInputActions.Gameplay.Look.canceled -= OnLook;
	}

	#endregion

	#region Controls

	// Global
	private void OnPause(InputAction.CallbackContext context) => PauseEvent.Invoke();

	// Gameplay
	private void OnPing(InputAction.CallbackContext context) => PingEvent.Invoke();

	private void OnBeaconHold(InputAction.CallbackContext context) => BeaconHoldEvent.Invoke(!context.canceled); // Invokes as true when the player starts to hold it, and invokes as false if they release.

	private void OnMove(InputAction.CallbackContext context) => MoveEvent.Invoke(context.ReadValue<Vector3>());

	private void OnLook(InputAction.CallbackContext context) => LookEvent.Invoke(context.ReadValue<Vector2>());

	#endregion

	#region Action Maps

	public void EnableGameplayControls() => m_playerInputActions?.Gameplay.Enable();

	public void DisableGameplayControls() => m_playerInputActions?.Gameplay.Disable();

	#endregion
}
