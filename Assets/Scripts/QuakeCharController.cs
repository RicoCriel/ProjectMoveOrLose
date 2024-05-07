using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Photon.Pun;
using System;

struct Cmd
{
    public float forwardMove;
    public float rightMove;
    public float upMove;
}

enum RobotState
{
    Idle,
    Running,
    Jumping,
}

public class QuakeCharController : MonoBehaviour
{
    public Transform playerView; // Camera
    public float playerViewYOffset = 0.8f; // The height at which the camera is bound to
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;

    /*Frame occuring factors*/
    public float gravity = 20.0f;
    public float friction = 6; //Ground friction

    /* Movement */
    public float moveSpeed = 7.0f; // Ground move speed
    public float runAcceleration = 14.0f; // Ground accel
    public float runDeacceleration = 10.0f; // Deacceleration that occurs when running on the ground
    public float airAcceleration = 2.0f; // Air accel
    public float airDecceleration = 2.0f; // Deacceleration experienced when ooposite strafing
    public float airControl = 0.3f; // How precise air control is
    public float sideStrafeAcceleration = 50.0f; // How fast acceleration occurs to get up to sideStrafeSpeed when
    public float sideStrafeSpeed = 1.0f; // What the max speed to generate when side strafing
    public float jumpSpeed = 8.0f; // The speed at which the character's up axis gains when hitting jump
    public bool holdJumpToBhop = false; // When enabled allows player to just hold jump button to keep on bhopping perfectly. 

    private CharacterController controller;

    // Camera rotations
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    // Used to display real time fricton values
    private float playerFriction = 0.0f;

    // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)
    private Cmd cmd;

    private PhotonView _photonView;

    // Bullets
    public int playerId;
    [SerializeField] private GameObject rocketLauncher;
    [SerializeField] private GameObject rocketBullet;
    [SerializeField] private GameObject rocketBulletExit;
    private Vector3 impact;

    [SerializeField] private Animator robotAnimator;

    public float RocketBulletSpeed = 40f;

    [SerializeField] private SkinnedMeshRenderer _robotMesh;
    private RobotState robotState = RobotState.Idle;
    private string previousState = "";
    private bool previousStateFlag;

    [SerializeField] private Shotgun shotGun;
    [SerializeField] private Canon canon;
    [SerializeField] private ExplosionManager explosionManager;


    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerView == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerView = mainCamera.gameObject.transform;
        }

        PlaceCameraInCollider();

        controller = GetComponent<CharacterController>();
        _photonView = GetComponent<PhotonView>();
        robotState = RobotState.Idle;

        if (_photonView.IsMine)
        {
            _robotMesh.enabled = false;
        }
    }

    private void Update()
    {
        if (_photonView.IsMine)
        {
            LockCursor();
            CameraAndWeaponRotation();

            QueueJump();
            if (controller.isGrounded)
                GroundMove();
            else if (!controller.isGrounded)
                AirMove();

            explosionManager.AddExplosionForce(ref impact, ref playerVelocity);
            if (playerVelocity.magnitude > 20f)
            {
                playerVelocity = playerVelocity.normalized * 20f;
            }

            controller.Move(playerVelocity * Time.deltaTime);

            /* Calculate top velocity */
            Vector3 udp = playerVelocity;
            udp.y = 0.0f;
            if (udp.magnitude > playerTopVelocity)
                playerTopVelocity = udp.magnitude;

            //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
            // Set the camera's position to the transform
            playerView.position = new Vector3(
                transform.position.x,
                transform.position.y + playerViewYOffset,
                transform.position.z);

            HandleShootingInput();
            UpdateStates();
            UpdateAnimation();
        }
    }

    private void PlaceCameraInCollider()
    {
        // Put the camera inside the capsule collider
        playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
        rocketLauncher.transform.position = playerView.position;
    }

    private void CameraAndWeaponRotation()
    {
        /* Camera rotation stuff, mouse controls this shit */
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        // Clamp the X rotation
        if (rotX < -90)
            rotX = -90;
        else if (rotX > 90)
            rotX = 90;

        this.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider
        playerView.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera
        rocketLauncher.transform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    private static void LockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private Dictionary<RobotState, string> stateToAnimation = new Dictionary<RobotState, string>()
    {
        { RobotState.Idle, "IsIdling" },
        { RobotState.Running, "IsRunning" },
        { RobotState.Jumping, "IsJumping" },
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
        bool isGrounded = controller.isGrounded;
        bool isJumping = playerVelocity.y > 1f;

        Dictionary<Func<bool>, RobotState> stateMappings = new Dictionary<Func<bool>, RobotState>()
        {
            { () => isGrounded && isMoving, RobotState.Running },
            { () => isGrounded && !isMoving, RobotState.Idle },
            { () => isMoving && wishJump, RobotState.Jumping },
            { () => isMoving && !isGrounded, RobotState.Jumping },
            { () => isJumping, RobotState.Jumping }
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
            if (canon.canShootCanon)
            {
                explosionManager.AddPush(-playerView.transform.forward * canon.CanonDirectionSpeed,
                    canon.CanonForce, playerVelocity, ref impact);
            }
            canon.Shoot(ref playerView, this.gameObject);

        }
        if (Input.GetButtonDown("Fire1"))
        {
            if (shotGun.canShootShotgun)
            {
                explosionManager.AddPush(-playerView.transform.forward * shotGun.ShotgunDirectionSpeed,
                    shotGun.ShotgunForce, playerVelocity, ref impact);
            }
            shotGun.Shoot();
        }
    }

    public void AddImpact(Vector3 explosionOrigin, float force)
    {
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

    /**
     * Queues the next jump just like in Q3
     */
    private void QueueJump()
    {
        if (holdJumpToBhop)
        {
            wishJump = Input.GetButton("Jump");
            return;
        }

        if (Input.GetButtonDown("Jump") && !wishJump)
            wishJump = true;
        if (Input.GetButtonUp("Jump"))
            wishJump = false;
    }

    /**
     * Execs when the player is in the air
    */
    private void AirMove()
    {
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

        if (!canon.IsCanonShooting && controller.isGrounded)
        {
            playerVelocity.y = 0;
            playerVelocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            playerVelocity.y -= gravity * Time.deltaTime;
        }
    }

    /**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
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
    }
    
    // private void AirControl(Vector3 wishdir, float wishspeed)
    // {
    //     float zspeed;
    //     float speed;
    //     float dot;
    //     float k;
    //
    //     // Can't control movement if not moving forward or backward
    //     if (Mathf.Abs(cmd.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
    //         return;
    //
    //     zspeed = playerVelocity.y;
    //     playerVelocity.y = 0;
    //
    //     // Calculate speed and normalize player velocity
    //     speed = playerVelocity.magnitude;
    //     playerVelocity.Normalize();
    //
    //     // Calculate dot product of player velocity and desired direction
    //     dot = Vector3.Dot(playerVelocity, wishdir);
    //
    //     // Adjust air control based on dot product
    //     k = 32;
    //     k *= airControl * dot * Time.deltaTime;
    //
    //     // Change direction while maintaining speed
    //     playerVelocity.x += wishdir.x * k;
    //     playerVelocity.z += wishdir.z * k;
    //
    //     // Normalize player velocity
    //     playerVelocity.Normalize();
    //
    //     // Restore original speed
    //     playerVelocity.x *= speed;
    //     playerVelocity.z *= speed;
    //
    //     // Restore original vertical speed
    //     playerVelocity.y = zspeed;
    // }

    private void GroundMove()
    {
        //// Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(0.5f);
        else
            ApplyFriction(0);

        SetMovementDir();
        Vector3 wishdir;
        wishdir = new Vector3(cmd.rightMove, 0, cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the gravity velocity
        playerVelocity.y = -gravity * Time.deltaTime;

        if (wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (controller.isGrounded)
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
        if (hit.normal == Vector3.down)
        {
            // Call your method when the character hits an object from below
            OnHeadHit();
        }
    }

    private void OnHeadHit()
    {
        Debug.Log("Head hit");
        playerVelocity.y = 0;
    }
}
