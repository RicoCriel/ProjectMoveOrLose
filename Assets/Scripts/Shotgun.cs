using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace;
using Photon.Realtime;
using System;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private Transform shotgunBulletExit;
    [SerializeField] private float shotgunRange;
    [SerializeField] private Animator shotgunAnimator;
    [SerializeField] private TrailRenderer bulletTrail;

    private ExplosionManager explosionManager;
    private float shotgunCountdown = 1f;

    public float ShotgunDirectionSpeed = 70f;
    public float ShotgunForce =  400f;
    public bool canShootShotgun = true;

    private void Awake()
    {
        explosionManager = GetComponent<ExplosionManager>();  
    }

    public void Shoot()
    {
        if (!canShootShotgun)
            return;

        RaycastHit hit;
        Vector3 rayDirection = shotgunBulletExit.transform.forward;
        Vector3 adjustedOrigin = shotgunBulletExit.transform.position + rayDirection * 0.1f;

        Debug.DrawRay(adjustedOrigin, rayDirection * shotgunRange, Color.red, 3);

        TrailRenderer trail = Instantiate(bulletTrail, adjustedOrigin, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, adjustedOrigin, rayDirection, shotgunRange));

        bool hitSomething = Physics.Raycast(adjustedOrigin, rayDirection, out hit, shotgunRange);
        if (hitSomething)
        {
            Debug.DrawRay(adjustedOrigin, rayDirection * shotgunRange, Color.green, 3);
            explosionManager.view.RPC("triggerEffectRPC", RpcTarget.All, hit.point);
            StartCoroutine(explosionManager.ExplosionAtPoint(hit.point));

            if (hit.transform.gameObject.GetComponent<PhotonView>() != null)
            {
                BombManager.instance.PushTarget(hit.transform.gameObject.GetComponent<PhotonView>().ViewID,
                    ShotgunForce, transform.position, explosionManager.explosionRadius);
            }
        }

        StartCoroutine(SpawnTrail(trail, adjustedOrigin, rayDirection, shotgunRange, hitSomething ? hit : null));

        canShootShotgun = false;
        StartCoroutine(ShotgunCooldown(shotgunCountdown));
        shotgunAnimator.SetTrigger("shot");
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 startPos, Vector3 direction, float range, RaycastHit? hitInfo = null)
    {
        float time = 0;
        Vector3 endPos;

        if (hitInfo.HasValue)
        {
            endPos = hitInfo.Value.point;
        }
        else
        {
            endPos = startPos + direction * range;
        }


        while (time < trail.time)
        {
            trail.transform.position = Vector3.Lerp(startPos, endPos, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = endPos;
        Destroy(trail.gameObject, trail.time);
    }

    IEnumerator ShotgunCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootShotgun = true;
        shotgunAnimator.ResetTrigger("shot");
    }


}
