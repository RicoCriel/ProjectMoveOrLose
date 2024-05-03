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

    [SerializeField] private ExplosionManager explosionManager;

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
            Debug.DrawRay(shotgunBulletExit.transform.position, shotgunBulletExit.transform.forward * shotgunRange, Color.red, 1);
            if (Physics.Raycast(shotgunBulletExit.transform.position, shotgunBulletExit.transform.forward, out hit, shotgunRange))
            {
                Debug.DrawRay(shotgunBulletExit.transform.position, shotgunBulletExit.transform.forward * shotgunRange, Color.green, 1);
                StartCoroutine(explosionManager.ExplosionAtPoint(hit.transform.position));
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
