using DefaultNamespace.PowerUps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Burst.CompilerServices;

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
    [SerializeField] private GameObject sparkEffect;
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
                photonView.RPC("SpawnSparks", RpcTarget.All, other.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
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
    }
}

