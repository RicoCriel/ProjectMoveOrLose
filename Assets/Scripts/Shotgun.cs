using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private Transform shotgunBulletExit;
    [SerializeField] private float shotgunRange;

    [SerializeField] private Animator shotgunAnimator;
    private ExplosionManager explosionManager;

    private float shotgunCountdown = 1f;
    private bool canShootShotgun = true;

    private void Awake()
    {
        explosionManager = GetComponent<ExplosionManager>();  
    }

    public void Shoot()
    {
        if (canShootShotgun)
        {
            RaycastHit hit;

            Vector3 rayDirection = shotgunBulletExit.transform.forward;

            // Calculate the adjusted origin of the ray (closer to the shotgun exit)
            Vector3 adjustedOrigin = shotgunBulletExit.transform.position + rayDirection * 0.1f; // Move 0.1 units along the forward direction

            Debug.DrawRay(adjustedOrigin, rayDirection * shotgunRange, Color.red, 3);

            if (Physics.Raycast(adjustedOrigin, rayDirection, out hit, shotgunRange))
            {
                Debug.DrawRay(adjustedOrigin, rayDirection * shotgunRange, Color.green, 3);
                explosionManager.view.RPC("triggerEffectRPC", RpcTarget.All, hit.point);
                StartCoroutine(explosionManager.ExplosionAtPoint(hit.point));
                //Debug.Log("Hit " + hit.collider.gameObject.name);
            }

            canShootShotgun = false;
            StartCoroutine(ShotgunCooldown(shotgunCountdown));
            shotgunAnimator.SetTrigger("shot");
        }
    }

    IEnumerator ShotgunCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootShotgun = true;
        shotgunAnimator.ResetTrigger("shot");
    }

}
