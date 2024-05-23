using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform robotArms;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] private Vector3 robotArmsOffset;
    [SerializeField] private Vector3 camOffset;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        RotateCamera();
        UpdateRobotArmsPosition();
    }

    void RotateCamera()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Quaternion playerRotation = playerMovement.transform.rotation;

        this.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        //this.transform.position = playerMovement.transform.position + camOffset;
        this.transform.rotation = playerRotation * this.transform.localRotation;
    }

    void UpdateRobotArmsPosition()
    {
        robotArms.localPosition = this.transform.localPosition + robotArmsOffset;
    }


}
