using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    public static Sonar Instance { get; private set; }

    [SerializeField] private Material material;

    [SerializeField] private float pingSpeed;
    [SerializeField] private float pingMaxDistance;

    [SerializeField] private Transform pingLight;

    private Vector3 pingOrigin;
    private float pingDistance;

    private bool pingEnabled = false;

    public bool PingEnabled { get { return pingEnabled; } }

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
        InputManager.Instance.pingEvent += Ping;
    }

    private void OnDisable()
    {
        InputManager.Instance.pingEvent -= Ping;
    }

    private void Start()
    {
        material.SetFloat("_PingMaxDistance", pingMaxDistance);
        material.SetFloat("_PingDistance", pingMaxDistance);
    }

    void Update()
    {
        if (pingEnabled)
        {
            if (pingDistance > pingMaxDistance)
            {
                pingEnabled = false;
            }
            else
            {
                pingDistance += pingSpeed * Time.deltaTime;
                material.SetFloat("_PingDistance", pingDistance);
            }
        } 
    }

    private void Ping()
    {
        if (!pingEnabled)
        {
            pingOrigin = transform.position;
            material.SetVector("_PingOrigin", pingOrigin);
            pingLight.position = pingOrigin;

            pingDistance = 0;
            material.SetFloat("_PingDistance", pingDistance);

            pingEnabled = true;
        }
    }
}
