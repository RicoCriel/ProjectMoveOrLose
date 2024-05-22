using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity;
    public Transform robotArms;
    public PlayerMovement playerMovement;
    public float rotationSpeed = 10f;
    public Vector3 robotArmsOffset;

    private float xRotation = 0f;
    private float currentZRotation;

    private Dictionary<PlayerMovement.GravityState, Quaternion> targetRotations = new Dictionary<PlayerMovement.GravityState, Quaternion>();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        targetRotations.Add(PlayerMovement.GravityState.Up, Quaternion.Euler(180, 0, 0));
        targetRotations.Add(PlayerMovement.GravityState.Down, Quaternion.Euler(0, 0, 0));
        targetRotations.Add(PlayerMovement.GravityState.Forward, Quaternion.Euler(-90, 0, 0));
        targetRotations.Add(PlayerMovement.GravityState.Backward, Quaternion.Euler(90, 0, 0));
        targetRotations.Add(PlayerMovement.GravityState.Left, Quaternion.Euler(0, 0, -90));
        targetRotations.Add(PlayerMovement.GravityState.Right, Quaternion.Euler(0, 0, 90));
    }

    void Update()
    {
        RotateCamera();
        //AlignCameraWithGravity();
        UpdateRobotArmsPosition();
    }

    void RotateCamera()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        this.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void AlignCameraWithGravity()
    {
        Quaternion targetRotation;

        if (targetRotations.TryGetValue(playerMovement.currentGravityState, out targetRotation))
        {
            currentZRotation = Mathf.LerpAngle(currentZRotation, targetRotation.eulerAngles.z, rotationSpeed * Time.deltaTime);

            // Set the new rotation only updating the Z component
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x,
                transform.localRotation.eulerAngles.y, currentZRotation);
        }
    }

    void UpdateRobotArmsPosition()
    {
        // Maintain the robot arms' offset relative to the camera
        robotArms.localPosition = robotArmsOffset;
        Quaternion targetRotation;

        if (targetRotations.TryGetValue(playerMovement.currentGravityState, out targetRotation))
        {
            //currentZRotation = Mathf.LerpAngle(currentZRotation, targetRotation.eulerAngles.z, rotationSpeed * Time.deltaTime);

            //robotArms.localRotation = Quaternion.Euler(robotArms.eulerAngles.x,
            //    robotArms.localRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        }
    }
}
