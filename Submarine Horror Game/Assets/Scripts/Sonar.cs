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

    private void Start()
    {
        material.SetFloat("_PingMaxDistance", pingMaxDistance);

        Ping();
    }

    void Update()
    {
        pingDistance += pingSpeed * Time.deltaTime;

        if (pingDistance > pingMaxDistance)
        {
            Ping();
        }
        else
        {
            material.SetFloat("_PingDistance", pingDistance);
        } 
    }

    private void Ping()
    {
        pingOrigin = transform.position;
        material.SetVector("_PingOrigin", pingOrigin);
        pingLight.position = pingOrigin;

        pingDistance = 0;
        material.SetFloat("_PingDistance", pingDistance);
    }
}
