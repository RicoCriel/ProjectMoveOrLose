using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum GravityState
    {
        Up,
        Down,
        Forward,
        Backward,
        Left,
        Right
    }

    public GravityState currentGravityState;
    public LayerMask groundLayer;

    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;
    public float jumpForce;
    public float gravityForce = 9.81f;
    private float yRotation = 0f;

    private Rigidbody rb;

    public Dictionary<GravityState, Vector3> gravityDirections;
    private Dictionary<GravityState, Vector3> velocityAxes;
    private Dictionary<GravityState, Quaternion> rotations;

    [SerializeField] private SkinnedMeshRenderer robotMesh;
    [SerializeField] private bool disableMesh;
    [SerializeField] private bool enableRandomGravity;
    [SerializeField] private float rotationTransitionSpeed;
    private Quaternion currentRotation;
    private Quaternion targetRotation;
    
    void Start()
    {
        if(disableMesh)
        {
            robotMesh.enabled = false;
        }
        rb = GetComponent<Rigidbody>();
        InitializeDictionaries();
        UpdateGravity();
        targetRotation = rotations[currentGravityState];
        if (enableRandomGravity)
        {
            StartCoroutine(RandomGravitySwitch());
        }
    }

    void InitializeDictionaries()
    {
        gravityDirections = new Dictionary<GravityState, Vector3>
        {
            { GravityState.Up, Vector3.up },
            { GravityState.Down, Vector3.down },
            { GravityState.Forward, Vector3.forward },
            { GravityState.Backward, Vector3.back },
            { GravityState.Left, Vector3.left },
            { GravityState.Right, Vector3.right }
        };

        velocityAxes = new Dictionary<GravityState, Vector3>
        {
            { GravityState.Up, Vector3.up },
            { GravityState.Down, Vector3.up },
            { GravityState.Forward, Vector3.forward },
            { GravityState.Backward, Vector3.forward },
            { GravityState.Left, Vector3.right },
            { GravityState.Right, Vector3.right }
        };

        rotations = new Dictionary<GravityState, Quaternion>
        {
            { GravityState.Up, Quaternion.Euler(180, 0, 0) },
            { GravityState.Down, Quaternion.Euler(0, 0, 0) },
            { GravityState.Forward, Quaternion.Euler(-90, 0, 0) },
            { GravityState.Backward, Quaternion.Euler(90, 0, 0) },
            { GravityState.Left, Quaternion.Euler(0, 0, -90) },
            { GravityState.Right, Quaternion.Euler(0, 0, 90) }
        };
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X");

        //yRotation += mouseX * rotateSpeed * Time.deltaTime;
        IsPlayerGrounded();


        Vector3 velocity = transform.right * moveHorizontal * moveSpeed + transform.forward * moveVertical * moveSpeed;
        if (velocityAxes[currentGravityState] == Vector3.up)
        {
            rb.velocity = velocity + rb.velocity.y * Vector3.up;
        }
        else if (velocityAxes[currentGravityState] == Vector3.forward)
        {
            rb.velocity = velocity + rb.velocity.z * Vector3.forward;
        }
        else if (velocityAxes[currentGravityState] == Vector3.right)
        {
            rb.velocity = velocity + rb.velocity.x * Vector3.right;
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsPlayerGrounded())
        {
            Jump();
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            SetGravityState((GravityState)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(GravityState)).Length));
        }
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");

        // Define a dictionary to map each gravity state to its corresponding rotation control
        Dictionary<GravityState, Action<float>> rotationControls = new Dictionary<GravityState, Action<float>>
        {
            { GravityState.Down, RotateY },
            { GravityState.Up, RotateY },
            { GravityState.Forward, RotateX },
            { GravityState.Backward, RotateX },
            { GravityState.Left, RotateX },
            { GravityState.Right, RotateX }
        };

        // Execute the rotation control based on the current gravity state
        rotationControls[currentGravityState]?.Invoke(mouseX);
    }

    void RotateY(float mouseX)
    {
        // Rotate around the y-axis
        transform.Rotate(0f, mouseX * rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    void RotateX(float mouseX)
    {
        //// Get the current local rotation
        //Vector3 eulerAngles = transform.eulerAngles;

        //// Adjust the x rotation based on mouse input
        //eulerAngles.x += mouseX * rotateSpeed * Time.deltaTime;

        //// Apply the updated rotation
        //transform.localRotation = Quaternion.Euler(eulerAngles);
        // Get the current rotation
        Quaternion currentRotation = transform.rotation;

        // Calculate the rotation around the x-axis in world space
        Quaternion rotationX = Quaternion.AngleAxis(mouseX * rotateSpeed * Time.deltaTime, Vector3.right);

        // Apply the rotation
        transform.rotation = rotationX * currentRotation;
    }

    void ApplyGravity()
    {
        Vector3 gravity = gravityDirections[currentGravityState].normalized * gravityForce;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    void UpdateGravity()
    {
        Physics.gravity = gravityDirections[currentGravityState] * gravityForce;
    }

    private IEnumerator RandomGravitySwitch()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            SetGravityState((GravityState)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(GravityState)).Length));
            Debug.Log("ChangedState");
        }
    }

    void Jump()
    {
        Vector3 oppositeGravityDirection = -gravityDirections[currentGravityState];
        rb.AddForce(oppositeGravityDirection * jumpForce, ForceMode.Impulse);
    }

    bool IsPlayerGrounded()
    {
        RaycastHit hit;
        Vector3 rayDirection = gravityDirections[currentGravityState];
        Debug.DrawRay(rb.position, rayDirection * 1f,Color.red);
        if(Physics.Raycast(rb.position, rayDirection, out hit, 1f, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator SmoothRotate()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = rotations[currentGravityState];
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * rotationTransitionSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
        }
        transform.rotation = endRotation;
        endRotation.y = yRotation;
    }

    public void SetGravityState(GravityState newGravityState)
    {
        if (newGravityState != currentGravityState)
        {
            currentGravityState = newGravityState;
            StartCoroutine(SmoothRotate());
            UpdateGravity();
        }
    }
}
