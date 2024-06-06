using System.Collections;
using UnityEngine;
using Photon.Pun;

public abstract class WeaponPickUpBase : MonoBehaviourPun
{
    protected float duration = 60f;
    protected abstract void GiveWeapon(PlayerMovement player);
    protected abstract void RemoveWeapon(PlayerMovement player);
    protected abstract Weapon GetWeapon(PlayerMovement player);


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView == null || !playerView.IsMine) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        WeaponController controller = player.GetComponentInChildren<WeaponController>();
        if (player != null && controller != null)
        {
            StartCoroutine(ActivateGunPickup(player, controller));
        }
    }

    private IEnumerator ActivateGunPickup(PlayerMovement player, WeaponController controller)
    {
        Weapon weapon = GetWeapon(player);

        if (controller.currentSecondaryWeapon == null)
        {
            GiveWeapon(player);
            controller.SetActiveSecondaryWeapon(weapon);
        }
        else
        {
            Debug.Log("A secondary weapon is already active. Cannot pick up a new one.");
        }

        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.enabled = false;

        yield return new WaitForSeconds(duration);

        if (controller.currentSecondaryWeapon == weapon)
        {
            RemoveWeapon(player);
            controller.RemoveActiveSecondaryWeapon(weapon);
        }

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

