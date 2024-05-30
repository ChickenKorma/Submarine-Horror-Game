using UnityEngine;

public class Ping : MonoBehaviour
{
	#region Variables

	[SerializeField] private Material m_material;
	[SerializeField] private GameObject m_creaturePrefab;

	private Transform m_creatureTransform;

	private GameObject m_creatureModel;

	private string m_enabledPropertyName;
	private string m_originPropertyName;
	private string m_distancePropertyName;
	private string m_maxDistancePropertyName;

	private float m_speed;
	private float m_distance;
	private float m_maxDistance;

	private float m_soundDuration;

	private bool m_isSubmarine;
	private bool m_isSetup;

	#endregion

	#region Unity

	private void Awake()
	{
		m_creatureTransform = CreatureBehaviour.Instance.transform;
	}

	void Update()
	{
		if (!m_isSetup)
			return;

		m_distance += m_speed * Time.deltaTime;
		m_material.SetFloat(m_distancePropertyName, m_distance);

		if (m_distance > m_maxDistance)
			Destroy(gameObject);
	}

	private void OnDestroy()
	{
		m_material.SetInt(m_enabledPropertyName, 0);
		Destroy(m_creatureModel);
	}

	#endregion

	#region Implementation

	public void Setup(string materialPropertyNamePrefix, float speed, float maxDistance, float soundDuration = 0)
	{
		m_enabledPropertyName = $"_{materialPropertyNamePrefix}PingEnabled";
		m_originPropertyName = $"_{materialPropertyNamePrefix}PingOrigin";
		m_distancePropertyName = $"_{materialPropertyNamePrefix}PingDistance";
		m_maxDistancePropertyName = $"_{materialPropertyNamePrefix}PingMaxDistance";

		m_speed = speed;
		m_maxDistance = maxDistance;
		m_soundDuration = soundDuration;

		m_distance = 0;

		m_material.SetInt(m_enabledPropertyName, 1);
		m_material.SetVector(m_originPropertyName, transform.position);
		m_material.SetFloat(m_distancePropertyName, m_distance);
		m_material.SetFloat(m_maxDistancePropertyName, m_maxDistance);

		CreatureBehaviour.Instance.AddSound(transform.position, 1, m_soundDuration);

		Vector3 creatureModelPosition = m_creatureTransform.position;
		Quaternion creatureModelRotation = m_creatureTransform.rotation * m_creaturePrefab.transform.rotation;

		m_creatureModel = Instantiate(m_creaturePrefab, creatureModelPosition, creatureModelRotation);

		m_isSetup = true;
	}

	#endregion
}
