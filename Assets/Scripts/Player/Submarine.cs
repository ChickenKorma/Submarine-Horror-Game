using UnityEngine;

public class Submarine : MonoBehaviour
{
    [SerializeField] private float thrust;

    [SerializeField] private float turnSpeed;

    private Vector3 movement;

    private Vector2 lookInput;

    private Rigidbody rb;

    private Vector2 xScreenLimit;
    private Vector2 yScreenLimit;

    private bool isLookPositional = true;

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
        InputManager.Instance.deviceChangedEvent += OnControlSchemeChange;
        InputManager.Instance.moveEvent += OnMoveInput;
        InputManager.Instance.lookPositionalEvent += OnLookPositional;
        InputManager.Instance.lookStickEvent += OnLookStick;
    }

    private void OnDisable()
    {
        InputManager.Instance.deviceChangedEvent -= OnControlSchemeChange;
        InputManager.Instance.moveEvent -= OnMoveInput;
        InputManager.Instance.lookPositionalEvent -= OnLookPositional;
        InputManager.Instance.lookStickEvent -= OnLookStick;
    }

    private void FixedUpdate()
    {
        float xRot = 0, yRot = 0;

        if (isLookPositional)
        {
            if (lookInput.x < xScreenLimit.x)
            {
                // Rotate left
                yRot = -1;
            }
            else if (lookInput.x > xScreenLimit.y)
            {
                // Rotate right
                yRot = 1;
            }

            if (lookInput.y < yScreenLimit.x)
            {
                // Rotate up
                xRot = 1;
            }
            else if (lookInput.y > yScreenLimit.y)
            {
                // Rotate down
                xRot = -1;
            }
        }
        else
        {
            xRot = -lookInput.y;
            yRot = lookInput.x;
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(new Vector3(xRot, yRot, 0) * turnSpeed * Time.fixedDeltaTime));

        rb.AddRelativeForce(movement * thrust);
    }

    private void OnControlSchemeChange()
    {
        isLookPositional = InputManager.Instance.currentControlScheme == ControlScheme.KeyboardAndMouse;
        lookInput = Vector2.zero;
    }

    private void OnMoveInput(Vector3 input) => movement = input;

    private void OnLookStick(Vector2 input)
    {
        isLookPositional = false;
        lookInput = input;
    }

    private void OnLookPositional(Vector2 input)
    {
        isLookPositional = true;
        lookInput = input;
    }
}
