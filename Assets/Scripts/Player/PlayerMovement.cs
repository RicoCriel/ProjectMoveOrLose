using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Threading;
using DefaultNamespace.PowerUps;
using Photon.Realtime;
using UnityEngine.VFX;

public class PlayerMovement : MonoBehaviour, IPunObservable
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

    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
    }

    public PlayerState playerState;
    public GravityState currentGravityState;
    public LayerMask groundLayer;

    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;

    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;

    [SerializeField] private float airControlFactor = 0.8f;
    [SerializeField] private float airAcceleration = 30f;

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
    private Vector3 lookDirection;

    [SerializeField] private SkinnedMeshRenderer robotMesh;
    [SerializeField] private bool disableMesh;
    [SerializeField] private bool enableRandomGravity;

    [SerializeField] private GameObject gravityCanvas;
    private GravityUI gravityUI;

    private Quaternion targetRotation;
    private Coroutine rotating;
    private Coroutine randomGravity;
    private Coroutine playerShot;
    private Coroutine gravityCooldown;

    private bool isFalling;
    private bool changingGravityState = false;
    private bool gravityOnCoolDown;

    private float fallStartTime;
    private float fallDuration;
    private float originalGravityForce;
    private float adjustedGravityForce;

    private float moveHorizontal, moveVertical;
    private float mouseX, mouseY;
    private bool jump;

    //rotation
    private bool isRotating = false;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float currentRotationLerp;

    [Header("RotationSpeedAndTreshHolds")]
    [SerializeField]
    [Range(0f, 10f)]
    private float _minrotationDistanceTreshHold = 5f;
    [SerializeField]
    [Range(5f, 100f)]
    private float _maxrotationDistanceTreshHold = 20f;
    [SerializeField]
    [Range(0.2f, 5f)]
    private float _minTimeToRotate = 0.5f;
    [SerializeField]
    [Range(0.5f, 10f)]
    private float _maxTimeToRotate = 2f;

    private float _actualRotationTime;

    private Vector3 remotePosition;
    private Quaternion remoteRotation;

    private Camera playerCamera;
    private CameraController cameraController;

    public bool IsInvincible;
    public bool IsShot;
    public PowerUpBase activePowerUp;

    public bool CanMove = true;
    public Player Player;
    
    public void EnablePlayer()
    {
        CanMove = true;
    }
    
    public void DisablePlayer()
    {
        CanMove = false;
    }

    public void InitializePlayer(Player localPlayer, Color color)
    {
        Player = localPlayer;
        // robotMesh.material.color = color;
    }
    
    void Start()
    {
        SetRotationtimers();
        playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        cameraController = GetComponentInChildren<CameraController>();

        if (view.IsMine)
        {
            robotMesh.enabled = false;
            gravityCanvas = PhotonNetwork.Instantiate(gravityCanvas.name, transform.position, Quaternion.identity);
            gravityUI = gravityCanvas.GetComponent<GravityUI>();
        }

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

        // Initialize remote position and rotation with the current values
        remotePosition = rb.position;
        remoteRotation = rb.rotation;

        SetGravityState((GravityState)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(GravityState)).Length));
    }
    private void SetRotationtimers()
    {

        if (_minrotationDistanceTreshHold > _maxrotationDistanceTreshHold)
        {
            _maxrotationDistanceTreshHold = _minrotationDistanceTreshHold;
            Debug.LogError("Min rotation distance treshhold can't be greater than max rotation distance treshhold");
        }
        if (_minTimeToRotate > _maxTimeToRotate)
        {
            _maxTimeToRotate = _minTimeToRotate;
            Debug.LogError("Min rotation time can't be greater than max rotation time");
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
            { GravityState.Left, Quaternion.Euler(45, 0, -90) },
            { GravityState.Right, Quaternion.Euler(0, 0, 90) }
        };
    }

    void Update()
    {
        if (!view.IsMine)
        {
            rb.position = Vector3.Lerp(rb.position, remotePosition, Time.deltaTime * 10);
            rb.rotation = Quaternion.Lerp(rb.rotation, remoteRotation, Time.deltaTime * 10);
        }

        if(view.IsMine)
        {
            HandleInput();
            UpdatePlayerState();
            SetGravityShotValue();
        }
    }

    void FixedUpdate()
    {
        if (view.IsMine)
        {
            UpdateGravity();
            HandleMovement();
            HandleRotation();
            if (isRotating)
            {
                if (TryFindFallDistance(gravityDirections[currentGravityState] * originalGravityForce, out float distance))
                {
                    _actualRotationTime = LerpRotationSpeedOnFallDistance(_minTimeToRotate, _maxTimeToRotate,
                        _minrotationDistanceTreshHold, _maxrotationDistanceTreshHold, distance);
                }
                else
                {
                    _actualRotationTime = _minTimeToRotate;
                }

                currentRotationLerp += (Time.deltaTime * rotationTransitionSpeed) / _actualRotationTime;
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, currentRotationLerp);

                if (currentRotationLerp >= 1f)
                {
                    transform.rotation = endRotation;
                    isRotating = false;
                }
            }
        }
    }

    private void HandleInput()
    {
        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        jump = Input.GetKey(KeyCode.Space);
        
        if (Input.GetKeyDown(KeyCode.E) && !IsShot)
        {
            HandleGravityChange();
        }
        mouseX = Input.GetAxis("Mouse X");
    }

    void HandleGravityChange()
    {
        if (!gravityOnCoolDown)
        {
            ApplyGravityChange();
        }
        else
        {
            gravityUI.DisplayGravityUI();
        }
    }

    void ApplyGravityChange()
    {
        SetGravityStateBasedOnLookDirection();
        cameraController.PlayLinesVFX();
        Vector3 oppositeGravityDirection = -gravityDirections[currentGravityState];
        rb.AddForce(oppositeGravityDirection * 2.5f, ForceMode.Impulse);
        gravityOnCoolDown = true;
    }

    void HandleMovement()
    {
        Vector3 targetVelocity = rb.velocity;
        Vector3 currentVelocity = rb.velocity;
        bool isGrounded = IsPlayerGrounded();

        Vector3 horizontalVelocity = transform.right * moveHorizontal * moveSpeed;
        Vector3 verticalVelocity = transform.forward * moveVertical * moveSpeed;

        if (isGrounded)
        {
            // Grounded movement logic
            if (moveHorizontal != 0 && moveVertical != 0)
            {
                targetVelocity = (horizontalVelocity + verticalVelocity).normalized * moveSpeed;
            }
            else if (moveHorizontal != 0)
            {
                targetVelocity = horizontalVelocity;
            }
            else if (moveVertical != 0)
            {
                targetVelocity = verticalVelocity;
            }
            else
            {
                // Apply deceleration when no input is given
                targetVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * deceleration);
            }
        }
        else
        {
            // Non-grounded movement logic (air control)
            Vector3 airControlVelocity = (horizontalVelocity + verticalVelocity) * airControlFactor;
            targetVelocity = currentVelocity + airControlVelocity;

            // Ensure air control doesn't make the character faster than grounded movement
            if (targetVelocity.magnitude > moveSpeed)
            {
                targetVelocity = targetVelocity.normalized * moveSpeed;
            }
        }

        // Interpolate towards target velocity
        Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * (isGrounded ? acceleration : airAcceleration));

        // Maintain current velocity component based on gravity state
        if (velocityAxes[currentGravityState] == Vector3.up)
        {
            newVelocity = new Vector3(newVelocity.x, currentVelocity.y, newVelocity.z);
        }
        else if (velocityAxes[currentGravityState] == Vector3.forward)
        {
            newVelocity = new Vector3(newVelocity.x, newVelocity.y, currentVelocity.z);
        }
        else if (velocityAxes[currentGravityState] == Vector3.right)
        {
            newVelocity = new Vector3(currentVelocity.x, newVelocity.y, newVelocity.z);
        }

        // Apply the calculated velocity to the rigidbody
        rb.velocity = newVelocity;

        // Handle jump
        if (jump && isGrounded)
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
            { GravityState.Right, true }
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
        Debug.DrawRay(rb.position, rayDirection * 1f, Color.red);
        if (Physics.Raycast(rb.position, rayDirection, out hit, 1f, groundLayer))
        {
            cameraController.StopLinesVFX();
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TryFindFallDistance(Vector3 newGravetyDirection, out float distance)
    {
        RaycastHit hit;

        // Debug.DrawRay(rb.position, newGravetyDirection * 1f,Color.red);
        if (Physics.Raycast(rb.position, newGravetyDirection, out hit, groundLayer))
        {
            distance = hit.distance;
            return true;
        }
        else
        {
            distance = 0;
            return false;
        }
    }

    public GravityState GetGravityState()
    {
        return currentGravityState;
    }

    private void UpdatePlayerState()
    {
        bool isMoving = moveHorizontal != 0 || moveVertical != 0;
        bool isGrounded = IsPlayerGrounded();
        // Calculate the velocity component opposite to the current gravity direction
        Vector3 oppositeGravityDirection = -gravityDirections[currentGravityState];
        float gravityVelocityComponent = Vector3.Dot(rb.velocity, oppositeGravityDirection);

        bool isJumping = gravityVelocityComponent > 1f;

        Dictionary<Func<bool>, PlayerState> stateMappings = new Dictionary<Func<bool>, PlayerState>()
        {
            { () => isGrounded && isMoving, PlayerState.Running },
            { () => isGrounded && !isMoving, PlayerState.Idle },
            { () => isMoving && jump, PlayerState.Jumping },
            { () => isMoving && !isGrounded, PlayerState.Jumping },
            { () => isJumping, PlayerState.Jumping }
        };

        foreach (var state in stateMappings)
        {
            if (state.Key())
            {
                playerState = state.Value;
                return;
            }
        }
    }

    private float LerpRotationSpeedOnFallDistance(float minFallSpeed, float maxFallSpeed, float minDistance, float maxDistance, float actualFallDistance)
    {
        if (actualFallDistance <= minDistance)
        {
            return minFallSpeed;
        }
        else if (actualFallDistance >= maxDistance)
        {
            return maxFallSpeed;
        }
        else
        {
            float t = Mathf.InverseLerp(minDistance, maxDistance, actualFallDistance);
            return Mathf.Lerp(minFallSpeed, maxFallSpeed, t);
        }
    }

    void SetGravityStateBasedOnLookDirection()
    {
        lookDirection = playerCamera.transform.forward;
        GravityState newGravityState = currentGravityState;

        float maxDot = -1f;

        foreach (var gravityDirection in gravityDirections)
        {
            float dot = Vector3.Dot(lookDirection, gravityDirection.Value);

            if (dot > maxDot)
            {
                maxDot = dot;
                newGravityState = gravityDirection.Key;
            }
        }

        SetGravityState(newGravityState);
    }

    public void SetGravityState(GravityState newGravityState)
    {
        if (newGravityState != currentGravityState)
        {
            currentGravityState = newGravityState;

            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = rotations[currentGravityState];
            Quaternion rotationDifference = Quaternion.FromToRotation(currentRotation * Vector3.up, targetRotation * Vector3.up);

            startRotation = currentRotation;
            endRotation = rotationDifference * currentRotation;
            currentRotationLerp = 0f;

            isRotating = true;
            if(gravityCooldown != null)
                StopCoroutine(GravityCoolDown(2f));

            gravityCooldown = StartCoroutine(GravityCoolDown(2f));
            ApplyGravity();
        }
    }

    private IEnumerator GravityCoolDown(float duration)
    {
        yield return new WaitForSeconds(duration);
        gravityOnCoolDown = false;
        gravityUI.HideGravityUI();
    }

    private void SetGravityShotValue()
    {
        if (IsShot)
        {
            gravityUI.DisplayGravityUI();
            if (playerShot != null)
            {
                StopCoroutine(HandleGravityShot(4f));
                playerShot = StartCoroutine(HandleGravityShot(4f));
            }
        }
    }

    private IEnumerator HandleGravityShot(float duration)
    {
       yield return new WaitForSeconds(duration);
       IsShot = false;
        gravityUI.HideGravityUI();
    }

    public void SetCurrentGravityAmmoType(GravityAmmoType ammoType)
    {
        currentGravityAmmoType = ammoType;
    }

    public GravityAmmoType GetCurrentGravityAmmoType()
    {
        return currentGravityAmmoType;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(currentGravityState);
        }
        else
        {
            // Network player, receive data
            remotePosition = (Vector3)stream.ReceiveNext();
            remoteRotation = (Quaternion)stream.ReceiveNext();
            currentGravityState = (GravityState)stream.ReceiveNext();
        }
    }
}
