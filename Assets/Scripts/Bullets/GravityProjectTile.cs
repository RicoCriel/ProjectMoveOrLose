using DefaultNamespace.PowerUps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GravityEffect
{
    Down,
    Up,
    Forward,
    Backward,
    Left,
    Right
}

public class GravityProjectTile : MonoBehaviourPun
{
    public GravityAmmoType ammotype;
    private Dictionary<GravityAmmoType, PlayerMovement.GravityState> gravityEffectMapping;
    private Coroutine killObject;

    private void Start()
    {
        InitializeGravityEffectMapping();
        if(killObject != null)
            StopCoroutine(killObject);

        killObject = StartCoroutine(KillMe(3f));
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null && gravityEffectMapping.TryGetValue(ammotype, out var newGravityState))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ChangeGravityState", RpcTarget.All, other.GetComponent<PhotonView>().ViewID, (int)newGravityState);
            }
        }
    }

    private void InitializeGravityEffectMapping()
    {
        gravityEffectMapping = new Dictionary<GravityAmmoType, PlayerMovement.GravityState>
        {
            { GravityAmmoType.Down, PlayerMovement.GravityState.Down },
            { GravityAmmoType.Up, PlayerMovement.GravityState.Up },
            { GravityAmmoType.Forward, PlayerMovement.GravityState.Forward },
            { GravityAmmoType.Backward, PlayerMovement.GravityState.Backward },
            { GravityAmmoType.Left, PlayerMovement.GravityState.Left },
            { GravityAmmoType.Right, PlayerMovement.GravityState.Right }
        };
    }

    [PunRPC]
    private void ChangeGravityState(int playerViewID, int newGravityState)
    {
        PhotonView playerPhotonView = PhotonView.Find(playerViewID);
        if (playerPhotonView != null)
        {
            PlayerMovement playerMovement = playerPhotonView.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetGravityState((PlayerMovement.GravityState)newGravityState);
            }
        }
    }

    private IEnumerator KillMe(float time)
    {
        yield return new WaitForSeconds(time);
            PhotonNetwork.Destroy(this.gameObject);
    }
}

