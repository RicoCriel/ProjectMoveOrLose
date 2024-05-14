using System.Collections;
using UnityEngine;
using Photon.Pun;

public abstract class PowerUpBase : MonoBehaviourPun
{
    protected float duration = 5f;  

    protected abstract void ApplyEffect(QuakeCharController player);
    protected abstract void RemoveEffect(QuakeCharController player);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                StartCoroutine(ActivatePowerUp(other.GetComponent<QuakeCharController>()));
            }
        }
    }

    private IEnumerator ActivatePowerUp(QuakeCharController player)
    {
        ApplyEffect(player);
        yield return new WaitForSeconds(duration);
        RemoveEffect(player);
        PhotonNetwork.Destroy(gameObject);  // Destroy the power-up object after use
    }
}
