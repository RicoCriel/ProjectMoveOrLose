using System.Collections;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace.PowerUps;

public class GravityProjectile : MonoBehaviourPun
{
    public GravityAmmoType ammoType;
    [SerializeField] private GameObject sparkEffect;
    [SerializeField] private float sphereRadius;
    [SerializeField] private float gravityRadius;

    private void Awake()
    {
        StartCoroutine(KillMe(2f));
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

        Vector3 direction = GetComponent<Rigidbody>().velocity.normalized;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphereRadius, direction, out hit, gravityRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                HandlePlayerCollision(hit.collider);
            }
        }
    }

    private void HandlePlayerCollision(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found on the player");
            return;
        }

        PlayerMovement.GravityState currentGravityState = playerMovement.GetGravityState();
        PlayerMovement.GravityState reversedGravityState = ReverseGravityState(currentGravityState);
        if (reversedGravityState != currentGravityState)
        {
            photonView.RPC("ChangeGravityState", RpcTarget.All, other.GetComponent<PhotonView>().ViewID, reversedGravityState);
            photonView.RPC("SpawnSparks", RpcTarget.All, other.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
        }

        PhotonNetwork.Destroy(this.gameObject);
    }

    private PlayerMovement.GravityState ReverseGravityState(PlayerMovement.GravityState currentState)
    {
        switch (currentState)
        {
            case PlayerMovement.GravityState.Down:
                return PlayerMovement.GravityState.Up;
            case PlayerMovement.GravityState.Up:
                return PlayerMovement.GravityState.Down;
            case PlayerMovement.GravityState.Forward:
                return PlayerMovement.GravityState.Backward;
            case PlayerMovement.GravityState.Backward:
                return PlayerMovement.GravityState.Forward;
            case PlayerMovement.GravityState.Left:
                return PlayerMovement.GravityState.Right;
            case PlayerMovement.GravityState.Right:
                return PlayerMovement.GravityState.Left;
            default:
                Debug.LogError($"Unknown gravity state: {currentState}");
                return currentState;
        }
    }

    [PunRPC]
    private void ChangeGravityState(int playerViewID, PlayerMovement.GravityState newGravityState)
    {
        PhotonView playerPhotonView = PhotonView.Find(playerViewID);
        if (playerPhotonView != null && playerPhotonView.IsMine)
        {
            PlayerMovement playerMovement = playerPhotonView.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetGravityState(newGravityState);
            }
        }
    }

    [PunRPC]
    private void SpawnSparks(Vector3 pos, Vector3 scale)
    {
        GameObject spark = Instantiate(sparkEffect, pos, Quaternion.identity);
        spark.transform.localScale = scale;
    }

    private IEnumerator KillMe(float time)
    {
        yield return new WaitForSeconds(time);
        PhotonNetwork.Destroy(this.gameObject);
        Debug.Log(this.gameObject.name + " Destroyed");
    }
}


