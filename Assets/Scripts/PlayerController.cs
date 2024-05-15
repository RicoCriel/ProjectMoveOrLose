using UnityEngine;
using Photon.Pun;
using System;
using static UnityEngine.LightAnchor;


enum PlayerState
{
    Idle,
    Running,
    Jumping,
}

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f;

    [SerializeField, Range(0f, 100f)]
    private float maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    private float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    private int maxAirJumps = 0;

    [SerializeField, Range(0f, 180f)]
    private float maxGroundAngle = 25f;

    [SerializeField, Range(0, 90)]
    private float maxStairsAngle = 50f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1;

    [SerializeField]
    LayerMask stairsMask = -1;

    [SerializeField, Range(0f, 100f)]
    private float maxSnapSpeed = 100f;

    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform weaponTransfrom;

    [SerializeField]
    private float xMouseSensitivity;
    [SerializeField]
    private float yMouseSensitivity;

    private int jumpPhase;
    private int stepsSinceLastGrounded;
    private int stepsSinceLastJump;

    private Rigidbody body;
    private Vector2 playerInput;
    private Vector3 velocity;
    private Vector3 desiredVelocity;
    private Vector3 contactNormal;
    private Vector3 steepNormal;
    private Vector3 jumpDirection;


    private bool desiredJump;
    private int groundContactCount;
    private int steepContactCount;
    private float minGroundDotProduct;
    private float minStairsDotProduct;

    private float rotX;
    private float rotY;
    private float playerViewYOffset = 0.8f;

    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        body = GetComponent<Rigidbody>();
        InitializeCamera();
    }

    void Update()
    {
        playerInput.x = 0f;
        playerInput.y = 0f;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        desiredJump |= Input.GetButtonDown("Jump");

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        RotateCamera();
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        cameraTransform.position = new Vector3(
                        transform.position.x,
                        transform.position.y + playerViewYOffset,
                        transform.position.z);
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;
        ClearState();
    }

    private void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastGrounded += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            else if (jumpPhase <= maxAirJumps)
            {
                jumpDirection = contactNormal;
            }
            else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
            {
                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                }
                jumpDirection = contactNormal;
            }
            else if (OnSteep)
            {
                jumpDirection = steepNormal;
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    private void Jump()
    {
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
        }
        else if (jumpPhase < maxAirJumps)
        {
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    private void AdjustVelocity()
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Project the camera vectors onto the contact plane to get the movement directions
        Vector3 xAxis = ProjectOnContactPlane(cameraRight).normalized;
        Vector3 zAxis = ProjectOnContactPlane(cameraForward).normalized;

        // Calculate the current velocity along the camera axes
        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        // Calculate the desired velocity based on player input
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        // Update the velocity along the camera axes
        velocity += xAxis * (newX -currentX) + zAxis * (newZ - currentZ);
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    private bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    private float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
    }

    private bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    private void RotateCamera()
    {
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity* 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        rotX = Mathf.Clamp(rotX, -90f, 90f);

        this.transform.rotation = Quaternion.Euler(0, rotY, 0); 
        cameraTransform.rotation = Quaternion.Euler(rotX, rotY, 0);
        weaponTransfrom.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    private void InitializeCamera()
    {
        Camera mainCamera = Camera.main;
        if(mainCamera != null)
        {
            cameraTransform = mainCamera.gameObject.transform;
        }
    }
}
