using UnityEngine;
using Photon.Pun;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 10f, maxClimbSpeed = 2f;

    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f;

    [SerializeField, Range(0f, 100f)]
    private float maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 100f)]
    private float maxClimbAcceleration = 20f;

    [SerializeField, Range(0f, 10f)]
    private float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    private int maxAirJumps = 0;

    [SerializeField, Range(0f, 180f)]
    private float maxGroundAngle = 25f;

    [SerializeField, Range(0, 90)]
    private float maxStairsAngle = 50f;

    [SerializeField, Range(90, 180)]
    float maxClimbAngle = 140f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1;

    [SerializeField]
    LayerMask stairsMask = -1;

    [SerializeField]
    LayerMask climbMask = -1;

    [SerializeField, Range(0f, 100f)]
    private float maxSnapSpeed = 100f;

    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform weaponTransform;

    [SerializeField]
    private float xMouseSensitivity;
    [SerializeField]
    private float yMouseSensitivity;

    [SerializeField]
    private SkinnedMeshRenderer robotRenderer;

    private int jumpPhase;
    private int stepsSinceLastGrounded, stepsSinceLastJump;

    private Rigidbody body, connectedBody, previousConnectedBody;
    private Vector2 playerInput;
    private Transform playerInputSpace;
    private Vector3 velocity, connectionVelocity;
    private Vector3 contactNormal, steepNormal, climbNormal, lastClimbNormal;
    private Vector3 jumpDirection;
    private Vector3 connectionWorldPosition, connectionLocalPosition;

    private Vector3 upAxis, rightAxis, forwardAxis;

    private bool desiredJump;
    private int groundContactCount, steepContactCount, climbContactCount;
    private float minGroundDotProduct, minStairsDotProduct, minClimbDotProduct; 

    private float rotX, rotY;
    private float playerViewYOffset = 0.8f;

    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;

    bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;

    private PhotonView view;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        view = GetComponent<PhotonView>();

        InitializeCamera();

        if (view.IsMine)
        {
            robotRenderer.enabled = false;
        }
    }

    void Update()
    {
        playerInput.x = 0f;
        playerInput.y = 0f;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

        desiredJump |= Input.GetButtonDown("Jump");

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
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        UpdateState();
        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

        if (Climbing)
        {
            velocity -=
                contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f)
        {
            velocity +=
                contactNormal *
                (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else
        {
            velocity += gravity * Time.deltaTime;
        }

        body.velocity = velocity;
        ClearState();
    }

    private void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (CheckClimbing() || OnGround || SnapToGround() || CheckSteepContacts())
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
            contactNormal = upAxis;
        }

        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    private void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = connectedBody.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    private void Jump(Vector3 gravity)
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
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
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
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
        Debug.Log($"Evaluating collision with: {collision.gameObject.name}, Layer: {layer}, MinDot: {minDot}, MinClimbDot: {minClimbDotProduct}");

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            Debug.Log($"Contact {i}: Normal: {normal}, UpDot: {upDot}");

            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNormal += normal;
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }

                if (upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0)
                {
                    climbContactCount += 1;
                    climbNormal += normal;
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }



    private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    private void AdjustVelocity()
    {
        float acceleration, speed;
        forwardAxis = cameraTransform.forward;
        rightAxis = cameraTransform.right;

        Vector3 xAxis, zAxis;
        if (Climbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        }
        else
        {
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        xAxis = ProjectDirectionOnPlane(xAxis, contactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, contactNormal);

        // Calculate the current velocity along the camera axes
        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        // Calculate the desired velocity based on player input
        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

        // Update the velocity along the camera axes
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount = climbContactCount = 0;
        contactNormal = steepNormal = climbNormal = Vector3.zero;
        connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
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

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
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
        connectedBody = hit.rigidbody;
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
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
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
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        rotX = Mathf.Clamp(rotX, -90f, 90f);

        this.transform.rotation = Quaternion.Euler(0, rotY, 0);
        cameraTransform.rotation = Quaternion.Euler(rotX, rotY, 0);
        weaponTransform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    private void InitializeCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.gameObject.transform;
        }
    }

    bool CheckClimbing()
    {
        if (Climbing)
        {
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if (upDot >= minGroundDotProduct)
                {
                    climbNormal = lastClimbNormal;
                }
            }
            groundContactCount = 1;
            contactNormal = climbNormal;
            return true;
        }
        return false;
    }
}
