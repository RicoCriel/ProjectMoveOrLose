using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;


public abstract class WeaponPickUpBase: MonoBehaviourPun
{
    protected float duration = 60f;
    protected abstract void GiveWeapon(PlayerMovement player);
    protected abstract void RemoveWeapon(PlayerMovement player);

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView == null || !playerView.IsMine) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        WeaponController controller = player.GetComponentInChildren<WeaponController>();
        if (player != null /*&& controller.activeSecondaryGun == null*/)
        {
            StartCoroutine(ActivateGunPickup(player, controller));
        }
    }

    private IEnumerator ActivateGunPickup(PlayerMovement player, WeaponController controller)
    {
        //controller.activeSecundaryGun = this;

        GiveWeapon(player);
        MeshRenderer meshRenderer = this.GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = false;

        yield return new WaitForSeconds(duration);

        RemoveWeapon(player);
        PhotonNetwork.Destroy(gameObject);
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            float rotationSpeed = 45f;
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        }
    }
}
