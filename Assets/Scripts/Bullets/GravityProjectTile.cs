using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace.PowerUps;

public class GravityProjectTile : MonoBehaviourPun
{
    public GravityAmmoType ammoType;
    [SerializeField] private GameObject sparkEffect;

    private const float gravityChangeCooldown = 0.5f;
    private Dictionary<int, float> playerHitTimestamps = new Dictionary<int, float>();

    private Dictionary<PlayerMovement.GravityState, PlayerMovement.GravityState> reversedGravityStates = new Dictionary<PlayerMovement.GravityState, PlayerMovement.GravityState>
    {
        { PlayerMovement.GravityState.Down, PlayerMovement.GravityState.Up },
        { PlayerMovement.GravityState.Up, PlayerMovement.GravityState.Down },
        { PlayerMovement.GravityState.Forward, PlayerMovement.GravityState.Backward },
        { PlayerMovement.GravityState.Backward, PlayerMovement.GravityState.Forward },
        { PlayerMovement.GravityState.Left, PlayerMovement.GravityState.Right },
        { PlayerMovement.GravityState.Right, PlayerMovement.GravityState.Left }
    };

    private void Awake()
    {
        StartCoroutine(KillMe(2f));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
            return;

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
            return;

        int playerViewID = other.GetComponent<PhotonView>().ViewID;
        if (!CanChangeGravityState(playerViewID))
            return;

        PlayerMovement.GravityState currentGravityState = playerMovement.GetGravityState();
        PlayerMovement.GravityState reversedGravityState = GetReversedGravityState(currentGravityState);
        if (reversedGravityState != currentGravityState)
        {
            photonView.RPC("ChangeGravityState", RpcTarget.All, playerViewID, (int)reversedGravityState);
            photonView.RPC("SpawnSparks", RpcTarget.All, other.transform.position, new Vector3(0.25f, 0.25f, 0.25f));
            playerHitTimestamps[playerViewID] = Time.time;
        }

        PhotonNetwork.Destroy(this.gameObject);
    }

    private bool CanChangeGravityState(int playerViewID)
    {
        float currentTime = Time.time;
        if (playerHitTimestamps.TryGetValue(playerViewID, out float lastHitTime))
        {
            if (currentTime - lastHitTime < gravityChangeCooldown)
            {
                Debug.Log($"Cooldown active. Ignoring hit on player {playerViewID}");
                return false;
            }
        }
        return true;
    }

    private PlayerMovement.GravityState GetReversedGravityState(PlayerMovement.GravityState currentState)
    {
        if (reversedGravityStates.TryGetValue(currentState, out PlayerMovement.GravityState reversedState))
        {
            return reversedState;
        }
        else
        {
            Debug.LogError($"Reversed gravity state not found for current state: {currentState}");
            return currentState; 
        }
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
                Debug.Log($"Changing gravity state to: {(PlayerMovement.GravityState)newGravityState} for player: {playerViewID}");
            }
            else
            {
                Debug.LogError($"PlayerMovement component not found for player: {playerViewID}");
            }
        }
        else
        {
            Debug.LogError($"PhotonView not found for playerViewID: {playerViewID}");
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
        Debug.Log(this.gameObject.name + "Destroyed");
    }
}




