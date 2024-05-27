using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviourPun
{
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform robotArms;
    [SerializeField] private Vector3 robotArmsOffset;
    private float xRotation = 0f;
    private bool canControl = true;

    private PhotonView view;
    private Camera mainCamera;

    void Start()
    {
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            mainCamera = Camera.main;
            mainCamera.transform.SetParent(transform); 
            mainCamera.transform.localPosition = Vector3.zero; 
            mainCamera.transform.localRotation = Quaternion.identity; 

            Cursor.lockState = CursorLockMode.Locked;
            canControl = true;
        }
        else
        {
            canControl = false;
        }
    }

    void LateUpdate()
    {
        if (view.IsMine && canControl)
        {
            RotateCamera();
            UpdateRobotArmsPosition();
        }
    }

    void RotateCamera()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    void UpdateRobotArmsPosition()
    {
        robotArms.localPosition = transform.localPosition + robotArmsOffset;
    }
}

