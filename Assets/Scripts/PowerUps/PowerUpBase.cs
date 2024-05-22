using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;

public abstract class PowerUpBase : MonoBehaviourPun
{
    protected float duration = 5f;  
    protected PhotonView _myView;
    
    public PowerUpType _myPowerUpType;

    protected abstract void ApplyEffect(QuakeCharController player);
    protected abstract void RemoveEffect(QuakeCharController player);

    private void Awake()
    {
        throw new NotImplementedException();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                StartCoroutine(ActivatePowerUp(other.GetComponent<QuakeCharController>()));
            }
        }
    }
    private IEnumerator ActivatePowerUp(QuakeCharController player)
    {
        ApplyEffect(player);
        yield return new WaitForSeconds(duration);
        RemoveEffect(player);
        PhotonNetwork.Destroy(gameObject);  
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