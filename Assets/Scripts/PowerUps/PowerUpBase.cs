using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

public abstract class PowerUpBase : MonoBehaviourPun
{
    protected float duration = 10f;
    protected PhotonView _myView;
    protected Canvas powerUpCanvas;
    protected Image powerUpImage;
    protected Image powerUpDurationImage;
    protected PlayerMovement player;

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
        if(!other.CompareTag("Player")) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView == null || !playerView.IsMine) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.activePowerUp == null)
        {
            StartCoroutine(ActivatePowerUp(player));
        }
    }

    private IEnumerator ActivatePowerUp(PlayerMovement player)
    {
        player.activePowerUp = this;

        ApplyEffect(player);
        MeshRenderer meshRenderer = this.GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = false;

        powerUpCanvas = player.GetComponentInChildren<Canvas>(true);
        if (powerUpCanvas != null)
        {
            powerUpCanvas.gameObject.SetActive(true);
            powerUpDurationImage = powerUpCanvas.GetComponentInChildren<Image>();

            if (powerUpDurationImage != null)
            {
                foreach (Transform child in powerUpDurationImage.transform)
                {
                    Image childImage = child.GetComponent<Image>();
                    if (childImage != null)
                    {
                        powerUpImage = childImage;
                        break;
                    }
                }

                if (powerUpImage != null)
                {
                    powerUpImage.sprite = powerUpSprite;
                    powerUpDurationImage.fillAmount = 1;

                    DOTween.To(() => powerUpDurationImage.fillAmount, x => powerUpDurationImage.fillAmount = x, 0, duration)
                           .SetEase(Ease.Linear)
                           .OnUpdate(() => powerUpDurationImage.color = Color.Lerp(Color.red, Color.green, powerUpDurationImage.fillAmount))
                           .OnComplete(() => powerUpCanvas.gameObject.SetActive(false));
                }
            }
        }

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
