using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviourPun, IPunObservable
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
        Cursor.lockState = CursorLockMode.Locked;

        if (view.IsMine)
        {
            mainCamera = Camera.main;
            mainCamera.transform.SetParent(transform);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
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
            //photonView.RPC("UpdateRobotArmsRotation", RpcTarget.Others, transform.localRotation);
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
        //robotArms.localRotation = transform.localRotation;
    }

    [PunRPC]
    void UpdateRobotArmsRotation(Quaternion newRotation)
    {
        transform.localRotation = newRotation;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.localRotation);
        }
        else
        {
            transform.localRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}


