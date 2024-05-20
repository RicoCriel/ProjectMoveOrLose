using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerView;
    private float rotX, rotY;
    public float playerViewYOffset = 0.8f; 
    public float xMouseSensitivity, yMouseSensitivity;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        InitializeCamera();
        PlaceCameraInCollider();
    }

    private void Update()
    {
        LockCursor();
        CameraAndWeaponRotation();
        UpdateCameraPosition();
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

    private void UpdateCameraPosition()
    {
        // Set the camera's position to the transform
        if (playerView != null)
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
        if (playerView != null)
        {
            playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
        }
    }

    private void CameraAndWeaponRotation()
    {
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        rotX = Mathf.Clamp(rotX, -90, 90);

        this.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotY, transform.rotation.eulerAngles.z); // Rotates the collider
        playerView.rotation = Quaternion.Euler(rotX, rotY, 0); // Rotates the camera
    }

    private static void LockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
