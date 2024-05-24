using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Threading;
using DefaultNamespace.PowerUps;

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
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;

    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityForce;
    [SerializeField] private float maxGravityForce;
    [SerializeField] private float rotationTransitionSpeed;
    public float gravityIncreaseRate;

    private float yRotation = 0f;

    public Rigidbody rb;
    private PhotonView view;

    public Dictionary<GravityState, Vector3> gravityDirections;
    private Dictionary<GravityState, Vector3> velocityAxes;
    private Dictionary<GravityState, Quaternion> rotations;
    private GravityAmmoType currentGravityAmmoType;

    [SerializeField] private SkinnedMeshRenderer robotMesh;
    [SerializeField] private bool disableMesh;
    [SerializeField] private bool enableRandomGravity;
    

    private Quaternion targetRotation;
    private Coroutine rotating;
    private Coroutine randomGravity;
    private bool isFalling;
    private bool changingGravityState = false;
    private float fallStartTime;
    private float fallDuration;
    private float originalGravityForce;
    private float adjustedGravityForce;

    private float moveHorizontal, moveVertical;
    private float mouseX, mouseY;
    private bool jump;
    
    private bool isRotating = false;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float rotationTime;


    [SerializeField] private CameraController cameraController;

    void Start()
    {
        if(disableMesh)
        {
            robotMesh.enabled = false;
        }

        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        
        InitializeDictionaries();
        UpdateGravity();
        targetRotation = rotations[currentGravityState];
        if (enableRandomGravity)
        {
            if (randomGravity != null)
                StopCoroutine(randomGravity);

            randomGravity = StartCoroutine(RandomGravitySwitch());
        }

        originalGravityForce = gravityForce;
        adjustedGravityForce = gravityForce;
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
        HandleInput();
        Debug.Log(gravityIncreaseRate);
    }
    void FixedUpdate()
    {
        UpdateGravity();
        HandleMovement();
        HandleRotation();
        if (isRotating)
        {
            rotationTime += Time.deltaTime * rotationTransitionSpeed;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, rotationTime);

            if (rotationTime >= 1f)
            {
                transform.rotation = endRotation;
                isRotating = false;
            }
        }
    }

    private void HandleInput()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        jump = Input.GetKey(KeyCode.Space);
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetGravityState((GravityState)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(GravityState)).Length));
        }
        mouseX = Input.GetAxis("Mouse X");
    }

    void HandleMovement()
    {
        IsPlayerGrounded();

        Vector3 targetVelocity = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized * moveSpeed;

        Vector3 currentVelocity = rb.velocity;
        Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * acceleration);

        if (velocityAxes[currentGravityState] == Vector3.up)
        {
            newVelocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
        }
        else if (velocityAxes[currentGravityState] == Vector3.forward)
        {
            newVelocity = new Vector3(newVelocity.x, newVelocity.y, rb.velocity.z);
        }
        else if (velocityAxes[currentGravityState] == Vector3.right)
        {
            newVelocity = new Vector3(rb.velocity.x, newVelocity.y, newVelocity.z);
        }

        if (moveHorizontal == 0 && moveVertical == 0)
        {
            newVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * deceleration);
        }

        rb.velocity = newVelocity;

        if (jump && IsPlayerGrounded())
        {
            Jump();
        }
    }

    public void HandleRotation()
    {
        // Define a dictionary to map each gravity state to a boolean value indicating whether mouseX should be inverted
        Dictionary<GravityState, bool> invertMouseX = new Dictionary<GravityState, bool>
        {
            { GravityState.Down, false },
            { GravityState.Up, true },
            { GravityState.Forward, true },
            { GravityState.Backward, false },
            { GravityState.Left, false },
            { GravityState.Right, true}
        };

        if (invertMouseX.ContainsKey(currentGravityState) && invertMouseX[currentGravityState])
        {
            mouseX *= -1f;
        }

        // Define a dictionary to map each gravity state to its corresponding rotation control method
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
        transform.Rotate(0f, mouseX * rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    void RotateX(float mouseX)
    {
        if (currentGravityState == GravityState.Forward || currentGravityState == GravityState.Backward)
        {
            // Rotate around the z-axis in world space
            transform.Rotate(0f, 0f, mouseX * rotateSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            // Rotate around the x-axis in world space
            transform.Rotate(mouseX * rotateSpeed * Time.deltaTime, 0f, 0f, Space.World);
        }
    }

    void UpdateGravity()
    {
        //Vector3 gravity = gravityDirections[currentGravityState].normalized * gravityForce;
        //rb.AddForce(gravity, ForceMode.Acceleration);

        if (!IsPlayerGrounded())
        {
            if (!isFalling)
            {
                isFalling = true;
                fallStartTime = Time.time;
            }

            float elapsedTime = Time.time - fallStartTime;
            float t = elapsedTime / gravityIncreaseRate;

            if (t > 1f)
            {
                t = 1f;
            }

            //Debug.Log($"Elapsed Time: {elapsedTime}, t: {t}, adjustedGravityForce: {adjustedGravityForce}");

            adjustedGravityForce = Mathf.Lerp(originalGravityForce, maxGravityForce, t);
        }
        else
        {
            if (isFalling)
            {
                isFalling = false;
                adjustedGravityForce = originalGravityForce;
            }
        }

        Vector3 gravity = gravityDirections[currentGravityState].normalized * adjustedGravityForce;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    void ApplyGravity()
    {
        Physics.gravity = gravityDirections[currentGravityState] * originalGravityForce;
    }

    private IEnumerator RandomGravitySwitch()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            SetGravityState((GravityState)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(GravityState)).Length));
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

    // IEnumerator SmoothRotate()
    // {
    //     Quaternion startRotation = transform.rotation;
    //     Quaternion endRotation = rotations[currentGravityState];
    //     float time = 0f;
    //
    //     while (time < 1f)
    //     {
    //         time += Time.deltaTime * rotationTransitionSpeed;
    //         transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
    //         yield return new WaitForSeconds(Time.deltaTime * rotationTransitionSpeed);
    //     }
    //     transform.rotation = endRotation;
    //     endRotation.y = yRotation;
    //     
    //     yield return new WaitForEndOfFrame();
    // }

    // public void SetGravityState(GravityState newGravityState)
    // {
    //     if (newGravityState != currentGravityState)
    //     {
    //         currentGravityState = newGravityState;
    //
    //         if(rotating != null)
    //             StopCoroutine(rotating);
    //
    //         rotating = StartCoroutine(SmoothRotate());
    //         ApplyGravity();
    //     }
    // }
    
    public void SetGravityState(GravityState newGravityState)
    {
        if (newGravityState != currentGravityState)
        {
            currentGravityState = newGravityState;

            // Initialize rotation parameters
            startRotation = transform.rotation;
            endRotation = rotations[currentGravityState];
            rotationTime = 0f;
            isRotating = true;

            ApplyGravity();
        }
    }

    public void SetCurrentGravityAmmoType(GravityAmmoType ammoType)
    {
        currentGravityAmmoType = ammoType;
    }

    public GravityAmmoType GetCurrentGravityAmmoType()
    {
        return currentGravityAmmoType;
    }
}
