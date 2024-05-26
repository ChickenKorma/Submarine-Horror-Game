using UnityEngine;

public class Submarine : MonoBehaviour
{
	#region Variables

	[SerializeField] private float m_thrust;
	[SerializeField] private float m_turnSpeed;

	private Rigidbody m_rb;

	private Vector3 m_movement;
	private Vector2 m_lookInput;

	private Vector2 m_xScreenLimit;
	private Vector2 m_yScreenLimit;

	private bool m_isLookPositional = true;

	#endregion

	#region Unity

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		m_xScreenLimit.x = Screen.width * 0.25f;
		m_xScreenLimit.y = Screen.width * 0.75f;

		m_yScreenLimit.x = Screen.height * 0.25f;
		m_yScreenLimit.y = Screen.height * 0.75f;
	}

	private void OnEnable()
	{
		InputManager.Instance.DeviceChangedEvent += OnControlSchemeChange;
		InputManager.Instance.MoveEvent += OnMoveInput;
		InputManager.Instance.LookPositionalEvent += OnLookPositional;
		InputManager.Instance.LookStickEvent += OnLookStick;
	}

	private void OnDisable()
	{
		InputManager.Instance.DeviceChangedEvent -= OnControlSchemeChange;
		InputManager.Instance.MoveEvent -= OnMoveInput;
		InputManager.Instance.LookPositionalEvent -= OnLookPositional;
		InputManager.Instance.LookStickEvent -= OnLookStick;
	}

	private void FixedUpdate()
	{
		float xRot = 0, yRot = 0;

		if (m_isLookPositional)
		{
			if (m_lookInput.x < m_xScreenLimit.x)
			{
				// Rotate left
				yRot = -1;
			}
			else if (m_lookInput.x > m_xScreenLimit.y)
			{
				// Rotate right
				yRot = 1;
			}

			if (m_lookInput.y < m_yScreenLimit.x)
			{
				// Rotate up
				xRot = 1;
			}
			else if (m_lookInput.y > m_yScreenLimit.y)
			{
				// Rotate down
				xRot = -1;
			}
		}
		else
		{
			xRot = -m_lookInput.y;
			yRot = m_lookInput.x;
		}

		m_rb.MoveRotation(m_rb.rotation * Quaternion.Euler(m_turnSpeed * Time.fixedDeltaTime * new Vector3(xRot, yRot, 0)));

		m_rb.AddRelativeForce(m_movement * m_thrust);
	}

	#endregion

	#region Control handling

	private void OnControlSchemeChange()
	{
		m_isLookPositional = InputManager.Instance.CurrentControlScheme == ControlScheme.KeyboardAndMouse;
		m_lookInput = Vector2.zero;
	}

	private void OnMoveInput(Vector3 input) => m_movement = input;

	private void OnLookStick(Vector2 input)
	{
		m_isLookPositional = false;
		m_lookInput = input;
	}

	private void OnLookPositional(Vector2 input)
	{
		m_isLookPositional = true;
		m_lookInput = input;
	}

	#endregion
}
