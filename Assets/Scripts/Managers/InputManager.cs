using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	#region Variables

	public static InputManager Instance { get; private set; }

	public PlayerInput PlayerInput;

	public ControlScheme CurrentControlScheme;

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
		// Device management
		UpdateControlScheme();
		PlayerInput.onControlsChanged += OnDeviceChanged;

		m_playerInputActions = new PlayerInputActions();
		m_playerInputActions.Gameplay.Enable();

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

		m_playerInputActions.Gameplay.Pause.performed += OnPause;
	}

	private void OnDisable()
	{
		// Device management
		PlayerInput.onControlsChanged -= OnDeviceChanged;

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

		m_playerInputActions.Gameplay.Pause.performed -= OnPause;
	}

	#endregion

	#region Control Schemes

	private void UpdateControlScheme()
	{
		CurrentControlScheme = PlayerInput.currentControlScheme switch
		{
			"KBM" => ControlScheme.KeyboardAndMouse,
			"Gamepad" => ControlScheme.Gamepad,
			_ => ControlScheme.KeyboardAndMouse,
		};

		DeviceChangedEvent.Invoke();
	}

	private void OnDeviceChanged(PlayerInput _) => UpdateControlScheme();

	#endregion

	#region Controls

	private void OnPing(InputAction.CallbackContext context) => PingEvent.Invoke();

	// Invokes as true when the player starts to hold it, and invokes as false if they release.
	private void OnBeaconHold(InputAction.CallbackContext context) => BeaconHoldEvent.Invoke(!context.canceled);

	private void OnMove(InputAction.CallbackContext context) => MoveEvent.Invoke(context.ReadValue<Vector3>());

	private void OnLook(InputAction.CallbackContext context) => LookEvent.Invoke(context.ReadValue<Vector2>());

	private void OnPause(InputAction.CallbackContext context) => PauseEvent.Invoke();

	#endregion
}

public enum ControlScheme
{
	KeyboardAndMouse,
	Gamepad
}

