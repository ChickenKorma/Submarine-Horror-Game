using UnityEngine;

public class Submarine : MonoBehaviour
{
	#region Variables

	[SerializeField] private float m_thrust;
	[SerializeField] private float m_turnSpeed;

	[SerializeField] private float m_collisionBuffer;
	private float m_lastCollisionTime;

	[SerializeField] private float m_collisionVolume;
	[SerializeField] private float m_collisionSoundDuration;

	private Rigidbody m_rb;

	private Vector3 m_movement;
	private Vector2 m_lookInput;

	#endregion

	#region Unity

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		InputManager.Instance.MoveEvent += OnMoveInput;
		InputManager.Instance.LookEvent += OnLook;
	}

	private void OnDisable()
	{
		InputManager.Instance.MoveEvent -= OnMoveInput;
		InputManager.Instance.LookEvent -= OnLook;
	}

	private void FixedUpdate()
	{
		m_rb.MoveRotation(m_rb.rotation * Quaternion.Euler(m_turnSpeed * Time.fixedDeltaTime * new Vector3(-m_lookInput.y, m_lookInput.x, 0)));

		m_rb.AddRelativeForce(m_movement * m_thrust);

		AudioManager.Instance.SetSubmarineMoving(m_movement.sqrMagnitude > 0.25f);
		AudioManager.Instance.SetSubmarineRotating(m_lookInput.sqrMagnitude > 0.25f);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Environment") && Time.time > m_lastCollisionTime + m_collisionBuffer)
		{
			m_lastCollisionTime = Time.time;

			CreatureBehaviour.Instance.AddSound(transform.position, m_collisionVolume, m_collisionSoundDuration);
			AudioManager.Instance.PlaySubmarineCrash();
		}
	}

	#endregion

	#region Control handling

	private void OnMoveInput(Vector3 input) => m_movement = input;

	private void OnLook(Vector2 input) => m_lookInput = input;

	#endregion
}
