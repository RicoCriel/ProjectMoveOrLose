using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;

public abstract class PowerUpBase : MonoBehaviourPun
{
    protected float duration = 10f;  
    protected PhotonView _myView;
    
    public PowerUpType _myPowerUpType;

    protected abstract void ApplyEffect(PlayerMovement player);
    protected abstract void RemoveEffect(PlayerMovement player);

    private void Awake()
    {
        _myView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                MeshRenderer meshRenderer = this.GetComponentInChildren<MeshRenderer>();
                meshRenderer.enabled = false;
                StartCoroutine(ActivatePowerUp(other.GetComponent<PlayerMovement>()));
            }
        }
    }
    private IEnumerator ActivatePowerUp(PlayerMovement player)
    {
        ApplyEffect(player);
        yield return new WaitForSeconds(duration);
        RemoveEffect(player);
        PhotonNetwork.Destroy(gameObject);  
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            // Rotate 1 degree per second around the up axis
            float rotationSpeed = 45f;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}

public enum PowerUpType
{
    SpeedBoost,
    Invincibility,
    reloadspeedBoost,
    DamageRangeBoost,
    GravityChange,
}