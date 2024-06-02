using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.UI;
using Photon.Realtime;

public abstract class PowerUpBase : MonoBehaviourPun
{
    protected float duration = 10f;
    protected PhotonView _myView;
    protected Canvas powerUpCanvas;
    protected Image powerUpImage;

    public PowerUpType _myPowerUpType;
    public Sprite powerUpSprite;

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
        powerUpCanvas = player.GetComponentInChildren<Canvas>(true);
        if(powerUpCanvas != null)
        {
            powerUpCanvas.gameObject.SetActive(true);
            powerUpImage = powerUpCanvas.GetComponentInChildren<Image>(true);
            powerUpImage.sprite = powerUpSprite;
        }
        
        yield return new WaitForSeconds(duration);

        RemoveEffect(player);
        powerUpCanvas.gameObject.SetActive(false);
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
