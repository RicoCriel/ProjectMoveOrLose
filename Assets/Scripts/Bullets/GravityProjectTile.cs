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
    public GravityAmmoType ammoType;
    [SerializeField] private GameObject sparkEffect;
    private Dictionary<PlayerMovement.GravityState, PlayerMovement.GravityState> reversedGravityEffectMapping;
    private Coroutine killObject;

    private void Start()
    {
        InitializeReversedGravityEffectMapping();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            PlayerMovement.GravityState currentGravityState = playerMovement.GetGravityState();
            if (reversedGravityEffectMapping.TryGetValue(currentGravityState, out var reversedGravityState))
            {
                Debug.Log($"Player hit. Current state: {currentGravityState}, Reversed state: {reversedGravityState}");

                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("Is Master Client, sending RPCs");
                    photonView.RPC("ChangeGravityState", RpcTarget.AllBuffered, other.GetComponent<PhotonView>().ViewID, (int)reversedGravityState);
                    photonView.RPC("SpawnSparks", RpcTarget.All, other.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
                }
            }
        }
    }

    private void InitializeReversedGravityEffectMapping()
    {
        reversedGravityEffectMapping = new Dictionary<PlayerMovement.GravityState, PlayerMovement.GravityState>
        {
            { PlayerMovement.GravityState.Down, PlayerMovement.GravityState.Up },
            { PlayerMovement.GravityState.Up, PlayerMovement.GravityState.Down },
            { PlayerMovement.GravityState.Forward, PlayerMovement.GravityState.Backward },
            { PlayerMovement.GravityState.Backward, PlayerMovement.GravityState.Forward },
            { PlayerMovement.GravityState.Left, PlayerMovement.GravityState.Right },
            { PlayerMovement.GravityState.Right, PlayerMovement.GravityState.Left }
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
                Debug.Log($"Changing gravity state to: {(PlayerMovement.GravityState)newGravityState} for player: {playerViewID}");
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
        Debug.Log("Projectile Destroyed");
    }
}