using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Photon.Pun;
using System;
using Unity.Burst.CompilerServices;

struct Cmd
{
    public float forwardMove;
    public float rightMove;
    public float upMove;
}

enum PlayerState
{
    Idle,
    Running,
    Jumping,
}

public enum GravityState
{
    Up,  //Gravity that pulls towards the top face.
    Down, //Gravity that pulls towards the bottom face.
    Forward, //Gravity: Gravity that pulls towards the front face.
    Backward, //Gravity: Gravity that pulls towards the back face.
    Left, //Gravity: Gravity that pulls towards the left face.
    Right, //Gravity: Gravity that pulls towards the right face.

}

public class QuakeCharController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerView; 
    private float rotX, rotY;
    public float playerViewYOffset = 0.8f; // The height at which the camera is bound to
    public float xMouseSensitivity, yMouseSensitivity;

    [Header("Gravity Settings")]
    public GravityState currentGravityState = GravityState.Down;
    private GravityState previousGravityState;
    public float gravity = 20.0f;
    [SerializeField] private Transform mesh;

    private Dictionary<GravityState, Quaternion> gravityRotations;
    private Dictionary<GravityState, Vector3> gravityDirections;
    private Dictionary<GravityState, (Vector3 forward, Vector3 right)> gravityAxesMapping;

    public Vector3 gravityDirection = Vector3.down;
    private float gravityTransitionTime = 0.1f; 
    private float gravityTransitionTimer = 0f;
    private float initialMeshYOffset = 0.6f;
    private bool isGravityChanging = false;

    [Header("Movement Settings")]
    public float friction = 6; //Ground friction
    public float moveSpeed = 7.0f; 
    public float runAcceleration = 14.0f; 
    public float runDeacceleration = 10.0f; 
    public float airAcceleration = 2.0f; 
    public float airDecceleration = 2.0f; // Deacceleration experienced when ooposite strafing
    public float airControl = 0.3f; // How precise air control is
    public float sideStrafeAcceleration = 50.0f; // How fast acceleration occurs to get up to sideStrafeSpeed when
    public float sideStrafeSpeed = 1.0f; // What the max speed to generate when side strafing
    public float jumpSpeed = 8.0f; // The speed at which the character's up axis gains when hitting jump
    public bool holdJumpToBhop = false; // When enabled allows player to just hold jump button to keep on bhopping perfectly. 
    [SerializeField] private Transform playerTransform;
    private Cmd cmd; // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)
    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;
    private float playerFriction = 0.0f; // Used to display real time fricton values
    private bool wishJump = false; // Q3: players can queue the next jump just before he hits the ground
    private CharacterController controller;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject weaponParent;
    [SerializeField] private GameObject rocketBullet;
    [SerializeField] private GameObject rocketBulletExit;
    public float RocketBulletSpeed = 40f;
    public Shotgun shotGun;
    public Canon canon;
    public ExplosionManager explosionManager;
    private Vector3 impact;

    [Header("Player Animation Properties")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private SkinnedMeshRenderer robotMesh;
    private AnimationState robotState = AnimationState.Idle;
    private string previousState = "";
    private bool previousStateFlag;
    public bool hideMesh;

    [Header("Photon Properties")]
    public int playerId;
    private PhotonView view;

    [Header("Powerup Properties")]
    public bool isInvincible = false;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        view = GetComponent<PhotonView>();

        //InitializeCamera();
        //PlaceCameraInCollider();
        InitializeGravityDictionaries();

        controller = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();
        robotState = AnimationState.Idle;
        previousGravityState = currentGravityState;
        if (meshYOffset.TryGetValue(currentGravityState, out float yOffset))
        {
            initialMeshYOffset = yOffset;
        }

        if (view.IsMine && hideMesh)
        {
            robotMesh.enabled = false;
        }
    }

    private void Update()
    {
        if (view.IsMine)
        {
            LockCursor();

            bool grounded = IsPlayerGrounded();
            //CameraAndWeaponRotation();

            QueueJump();
            if (grounded)
            {
                GroundMove();
            }
            else
            {
                AirMove();
            }

            explosionManager.AddExplosionForce(ref impact, ref playerVelocity);

            if (previousGravityState != currentGravityState)
            {
                isGravityChanging = true;
                gravityTransitionTimer += Time.deltaTime;
                if (gravityTransitionTimer >= gravityTransitionTime)
                {
                    gravityTransitionTimer = 0f;
                    previousGravityState = currentGravityState;
                    
                    UpdateGravityState();
                }
            }

            if (isGravityChanging)
            {
                UpdateGravityRotation();
            }

            ApplyCustomGravity();
            CalculateTopVelocity();

            controller.Move(playerVelocity * Time.deltaTime);

            UpdateCameraPosition();
            //HandleShootingInput();
            UpdateStates();
            
            UpdateAnimation();
        }
    }

    private void InitializeCamera()
    {
        if (playerView == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerView = mainCamera.gameObject.transform;
        }
    }

    private void CalculateTopVelocity()
    {
        /* Calculate top velocity */
        Vector3 udp = playerVelocity;
        udp.y = 0.0f;
        if (udp.magnitude > playerTopVelocity)
            playerTopVelocity = udp.magnitude;
    }

    private void UpdateCameraPosition()
    {
        // Set the camera's position to the transform
        if(playerView != null)
        {
            playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
        }
    }

    private void PlaceCameraInCollider()
    {
        // Put the camera inside the capsule collider
        if(playerView != null)
        {
            playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
            weaponParent.transform.position = playerView.position;
        }
    }

    private void CameraAndWeaponRotation()
    {
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        rotX = Mathf.Clamp(rotX, -90, 90);

        this.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotY, transform.rotation.eulerAngles.z); // Rotates the collider
        //playerView.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera
        //weaponParent.transform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    private static void LockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private Dictionary<AnimationState, string> stateToAnimation = new Dictionary<AnimationState, string>()
    {
        { AnimationState.Idle, "IsIdling" },
        { AnimationState.Running, "IsRunning" },
        { AnimationState.Jumping, "IsJumping" },
    };

    private void UpdateAnimation()
    {
        string nextState = stateToAnimation[robotState];
        bool nextStateFlag = !string.IsNullOrEmpty(nextState);

        if (previousStateFlag)
        {
            robotAnimator.SetBool(previousState, false);
        }

        if (nextStateFlag)
        {
            robotAnimator.SetBool(nextState, true);
        }

        previousStateFlag = nextStateFlag;
        previousState = nextState;
    }

    private void UpdateStates()
    {
        bool isMoving = cmd.rightMove != 0 || cmd.forwardMove != 0;
        bool isGrounded = IsPlayerGrounded();
        bool isJumping = playerVelocity.y > 1f;

        Dictionary<Func<bool>, AnimationState> stateMappings = new Dictionary<Func<bool>, AnimationState>()
        {
            { () => isGrounded && isMoving, AnimationState.Running },
            { () => isGrounded && !isMoving, AnimationState.Idle },
            { () => isMoving && wishJump, AnimationState.Jumping },
            { () => isMoving && !isGrounded, AnimationState.Jumping },
            { () => isJumping, AnimationState.Jumping }
        };

        foreach (var state in stateMappings)
        {
            if (state.Key())
            {
                robotState = state.Value;
                return;
            }
        }
    }

    private void HandleShootingInput()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            HandleCanonFire();
            canon.Shoot(ref playerView /*this.gameObject*/);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (shotGun.canShootShotgun)
            {
                //explosionManager.AddPush(-playerView.transform.forward * shotGun.ShotgunDirectionSpeed,
                //    shotGun.ShotgunForce, playerVelocity, ref impact);
            }
            shotGun.Shoot();
        }
    }

    public void AddImpact(Vector3 explosionOrigin, float force)
    {
        if (isInvincible) return;

        Vector3 dir = this.transform.position - explosionOrigin;
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground

        // Subtract player's current velocity from the impact force
        Vector3 adjustedImpact = dir.normalized * force / 3 - playerVelocity;

        // Add the adjusted impact to the current impact
        impact += adjustedImpact;
    }

    public void AddPush(Vector3 direction, float force)
    {
        if (isInvincible) return;

        Vector3 dir = direction;
        // Subtract player's current velocity from the impact force
        Vector3 adjustedImpact = dir.normalized * force / 3 - playerVelocity;
        // Add the adjusted impact to the current impact
        impact += adjustedImpact;
    }

    private void SetMovementDir()
    {
        cmd.forwardMove = Input.GetAxisRaw("Vertical");
        cmd.rightMove = Input.GetAxisRaw("Horizontal");
    }
    
    private void QueueJump()
    {
       ////Queues the next jump just like in Q3
       // if (holdJumpToBhop)
       // {
       //     wishJump = Input.GetButton("Jump");
       //     return;
       // }

       // if (Input.GetButtonDown("Jump") && !wishJump)
       // {
       //     wishJump = true;
       // }

       // if (Input.GetButtonUp("Jump"))
       // {
       //     wishJump = false;
       // }

        wishJump = Input.GetButton("Jump");
    }

    private void AirMove()
    {
        //When the player is in the air
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;

        SetMovementDir();

        wishdir = new Vector3(cmd.rightMove, 0, cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;
        // If the player is ONLY strafing left or right
        if (cmd.forwardMove == 0 && cmd.rightMove != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (airControl > 0)
            AirControl(wishdir, wishspeed2);

        // Apply gravity based on gravity direction
        playerVelocity += gravityDirection * gravity * Time.deltaTime;

        //if (!canon.IsCanonShooting && controller.isGrounded)
        //{
        //    playerVelocity.y = 0;
        //    playerVelocity.y -= gravity * Time.deltaTime;
        //}
        //else
        //{
        //    playerVelocity.y -= gravity * Time.deltaTime;
        //}
    }

    private void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(cmd.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
        //Air control occurs when the player is in the air,
        //it allows players to move side to side much faster rather than being'sluggish' when it comes to cornering.

         //Adjust movement direction based on gravity direction
        Vector3 adjustedForward = Vector3.ProjectOnPlane(playerView.forward, gravityDirection).normalized;
        Vector3 adjustedRight = Vector3.ProjectOnPlane(playerView.right, gravityDirection).normalized;

        // Calculate movement input relative to gravity direction
        float inputForward = Input.GetAxisRaw("Vertical");
        float inputRight = Input.GetAxisRaw("Horizontal");

        // Calculate movement vector
        wishdir = (adjustedForward * inputForward + adjustedRight * inputRight).normalized;
        wishdir = transform.TransformDirection(wishdir);

        // Adjust speed based on gravity direction
        wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        // CPM: Aircontrol
        float accel;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;

        // Accelerate player
        Accelerate(wishdir, wishspeed, accel);

        // Apply gravity differently based on gravity direction
        if (gravityDirection == Vector3.up)
            playerVelocity.y -= gravity * Time.deltaTime;
        else if (gravityDirection == Vector3.down)
            playerVelocity.y += gravity * Time.deltaTime;
    }

    private void GroundMove()
    {
        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(0.5f);
        else
            ApplyFriction(0f);

        //SetMovementDir();
        //Vector3 wishdir;
        //wishdir = new Vector3(cmd.rightMove, 0, cmd.forwardMove);
        //wishdir = transform.TransformDirection(wishdir);
        //wishdir.Normalize();
        //moveDirectionNorm = wishdir;

        //var wishspeed = wishdir.magnitude;
        //wishspeed *= moveSpeed;

        //Accelerate(wishdir, wishspeed, runAcceleration);

        //// Reset the gravity velocity
        //playerVelocity.y = -gravity * Time.deltaTime;

        //if (wishJump)
        //{
        //    playerVelocity.y = jumpSpeed;
        //    wishJump = false;
        //}

        SetMovementDir();
        Vector3 wishdir = Vector3.zero;


        // Adjust forward and right vectors based on the gravity direction using the dictionary
        Vector3 forwardVector = gravityAxesMapping[currentGravityState].forward;
        Vector3 rightVector = gravityAxesMapping[currentGravityState].right;

        // Transform world space vectors to local space based on object's rotation
        Vector3 localForward = transform.TransformDirection(forwardVector);
        Vector3 localRight = transform.TransformDirection(rightVector);

        // Calculate the movement direction using local forward and right vectors and input axes
        wishdir += localForward * cmd.forwardMove;
        wishdir += localRight * cmd.rightMove;
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();

        float wishspeed = wishdir.magnitude * moveSpeed;
        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the component of the gravity direction velocity
        playerVelocity = Vector3.ProjectOnPlane(playerVelocity, gravityDirection);

        if (wishJump)
        {
            // Adjust jump direction based on gravity state
            Vector3 jumpDirection = -gravityDirection * jumpSpeed;
            playerVelocity += jumpDirection;

            wishJump = false;
        }

        Debug.Log("Forward Vector: " + forwardVector);
        Debug.Log("Right Vector: " + rightVector);
        Debug.Log("Wish Direction: " + wishdir);
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;
        float speed;
        float newspeed;
        float control;
        float drop = 0.0f;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        if (IsPlayerGrounded())
        {
            control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        playerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
        
     
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //if (hit.normal == Vector3.down)
        //{
        //    // Call your method when the character hits an object from below
        //    OnHeadHit();
        //}
    }

    private void OnHeadHit()
    {
        //Debug.Log("Head hit");
        playerVelocity.y = 0;
    }

    private void HandleCanonFire()
    {
        float playerYPosition = transform.position.y;
        float maxYPosition = 25.0f; 

        if (playerYPosition > maxYPosition)
        {
            if(canon.canShootCanon)
            {
                //explosionManager.AddPush(-playerView.transform.forward * 1,
                //    1, playerVelocity, ref impact);
            }
        }
        else
        {
            if (canon.canShootCanon)
            {
                //explosionManager.AddPush(-playerView.transform.forward * canon.CanonDirectionSpeed,
                //    canon.CanonForce, playerVelocity, ref impact);
            }
        }
    }

    private void InitializeGravityDictionaries()
    {
        gravityRotations = new Dictionary<GravityState, Quaternion>
        {
            { GravityState.Up, Quaternion.Euler(0, 0, 180) },
            { GravityState.Down, Quaternion.identity },
            { GravityState.Forward, Quaternion.Euler(90, 0, -180) },
            { GravityState.Backward, Quaternion.Euler(90, 0, 0) },
            { GravityState.Left, Quaternion.Euler(0, 0, -90) },
            { GravityState.Right, Quaternion.Euler(0, 0, 90) }
        };

        gravityDirections = new Dictionary<GravityState, Vector3>
        {
            { GravityState.Up, Vector3.up },
            { GravityState.Down, Vector3.down },
            { GravityState.Forward, Vector3.forward },
            { GravityState.Backward, Vector3.back },
            { GravityState.Left, Vector3.left },
            { GravityState.Right, Vector3.right }
        };

        gravityAxesMapping = new Dictionary<GravityState, (Vector3 forward, Vector3 right)>
        {
            { GravityState.Up, (Vector3.forward, Vector3.right) },
            { GravityState.Down, (Vector3.forward, Vector3.right) },
            { GravityState.Forward, (Vector3.forward, Vector3.right) },
            { GravityState.Backward, (Vector3.forward, Vector3.right) },
            { GravityState.Left, (Vector3.right, Vector3.forward) },
            { GravityState.Right, (Vector3.forward, Vector3.forward) }
        };
    }

    private readonly Dictionary<GravityState, float> raycastLengths = new Dictionary<GravityState, float>
    {
        { GravityState.Up, 1.2f },
        { GravityState.Down, 1.2f },
        { GravityState.Forward, 1.4f },
        { GravityState.Backward, 1.4f },
        { GravityState.Left, 1.4f },
        { GravityState.Right, 1.4f }
    };

    private readonly Dictionary<GravityState, float> meshYOffset = new Dictionary<GravityState, float>
    {
        { GravityState.Up, 0.6f },
        { GravityState.Down, 0f },
        { GravityState.Forward, 0.6f },
        { GravityState.Backward, 0.6f },
        { GravityState.Left, 0.6f },
        { GravityState.Right,0.6f }
    };

    private void ApplyCustomGravity()
    {
        playerVelocity += gravityDirection * gravity * Time.deltaTime;
    }

    private bool IsPlayerGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(controller.bounds.center, gravityDirection, out hit, raycastLengths[currentGravityState]))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateGravityState()
    {
        SetGravityDirection();
        UpdateGravityRotation();
        UpdateForwardAndRightAxes();

        ApplyMeshYOffset(initialMeshYOffset);

        // Update the previous gravity state for the next transition
        previousGravityState = currentGravityState;
    }

    private void UpdateGravityRotation()
    {
        if (gravityRotations.TryGetValue(currentGravityState, out Quaternion rotation))
        {
            transform.localRotation = rotation;
        }
    }

    private void SetGravityDirection()
    {
        if (gravityDirections.TryGetValue(currentGravityState, out Vector3 direction))
        {
            gravityDirection = direction;
        }
    }

    private void UpdateForwardAndRightAxes()
    {
        if (gravityAxesMapping.TryGetValue(currentGravityState, out var axes))
        {
            // Transform world space vectors to local space based on object's rotation
            Vector3 localForward = transform.TransformDirection(axes.forward);
            Vector3 localRight = transform.TransformDirection(axes.right);

            // Set the transformed vectors as forward and right
            transform.forward = localForward;
            transform.right = localRight;
        }
        else
        {
            Debug.LogWarning("Gravity state axes mapping not found.");
        }
    }

    private void ApplyMeshYOffset(float yOffset)
    {
        Vector3 meshPosition = mesh.localPosition;
        meshPosition.y = meshPosition.y + yOffset;
        mesh.localPosition = meshPosition;
    }


}







