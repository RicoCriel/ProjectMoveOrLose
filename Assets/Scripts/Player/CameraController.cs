using UnityEngine;
using Photon.Pun;
using UnityEngine.VFX;

public class CameraController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform robotArms;
    [SerializeField] private Vector3 robotArmsOffset;
    private float xRotation = 0f;
    private bool canControl = true;

    private PhotonView view;
    private Camera mainCamera;
    [SerializeField]
    private GameObject lines;
    private VisualEffect lineVFX;

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
            lines = PhotonNetwork.Instantiate(lines.name, mainCamera.transform.position, Quaternion.identity);
            lines.transform.SetParent(mainCamera.transform);
            lines.transform.localPosition = new Vector3(0,0,50f);
            lines.transform.localRotation = Quaternion.identity;
            lineVFX = lines.GetComponent<VisualEffect>();

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
            photonView.RPC("UpdateRobotArmsRotation", RpcTarget.Others, transform.localRotation);
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

    public void PlayLinesVFX()
    {
        if (lines != null)
        {
            lineVFX.Play();
        }
    }

    public void StopLinesVFX()
    {
        if(lines != null)
        {
            lineVFX.Stop();
        }
    }
}


