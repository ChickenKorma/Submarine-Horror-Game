using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine : MonoBehaviour
{
    [SerializeField] private float thrust;

    [SerializeField] private float turnSpeed;

    private Vector3 movement;

    private Vector2 mousePos;

    private Rigidbody rb;

    private Vector2 xScreenLimit;
    private Vector2 yScreenLimit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        xScreenLimit.x = Screen.width * 0.25f;
        xScreenLimit.y = Screen.width * 0.75f;

        yScreenLimit.x = Screen.height * 0.25f;
        yScreenLimit.y = Screen.height * 0.75f;
    }

    private void OnEnable()
    {
        InputManager.Instance.moveEvent += OnMoveInput;
        InputManager.Instance.lookEvent += OnLook;
    }

    private void OnDisable()
    {
        InputManager.Instance.moveEvent -= OnMoveInput;
        InputManager.Instance.lookEvent -= OnLook;
    }

    private void FixedUpdate()
    {
        float xRot = 0, yRot = 0;

        if (mousePos.x < xScreenLimit.x)
        {
            // Rotate left
            yRot = -1;
        }
        else if (mousePos.x > xScreenLimit.y)
        {
            // Rotate right
            yRot = 1;
        }

        if (mousePos.y < yScreenLimit.x)
        {
            // Rotate up
            xRot = 1;
        }
        else if (mousePos.y > yScreenLimit.y)
        {
            // Rotate down
            xRot = -1;
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(xRot, yRot, 0) * turnSpeed * Time.fixedDeltaTime));

        rb.AddRelativeForce(movement * thrust);
    }

    private void OnMoveInput(Vector3 input) => movement = input;

    private void OnLook(Vector2 input) => mousePos = input;
}
