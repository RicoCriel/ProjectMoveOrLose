using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace;
using Photon.Realtime;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private Transform shotgunBulletExit;
    [SerializeField] private float shotgunRange;
    [SerializeField] private Animator shotgunAnimator;
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private int shotgunPellets = 5;
    [SerializeField] private float shotgunSpreadAngle = 10f;

    private ExplosionManager explosionManager;
    private float shotgunCountdown = 1f;

    public float ShotgunDirectionSpeed = 70f;
    public float ShotgunForce =  400f;
    public bool canShootShotgun = true;

    private PhotonView view;

    private void Awake()
    {
        explosionManager = GetComponent<ExplosionManager>();
        view = GetComponent<PhotonView>();
    }

    public void Shoot()
    {
        if (!canShootShotgun)
            return;

        RaycastHit hit;
        Vector3 rayDirection = shotgunBulletExit.transform.forward;
        Vector3 adjustedOrigin = shotgunBulletExit.transform.position + rayDirection * 0.1f;

        for (int i = 0; i < shotgunPellets; i++)
        {
            Vector3 spreadDirection = Quaternion.Euler(Random.Range(-shotgunSpreadAngle, shotgunSpreadAngle),
                Random.Range(-shotgunSpreadAngle, shotgunSpreadAngle), 0) * rayDirection;

            Debug.DrawRay(adjustedOrigin, spreadDirection * shotgunRange, Color.red, 3);

            if (Physics.Raycast(adjustedOrigin, spreadDirection, out hit, shotgunRange))
            {
                Debug.DrawRay(adjustedOrigin, spreadDirection * shotgunRange, Color.green, 3);
                explosionManager.view.RPC("triggerEffectRPC", RpcTarget.All, hit.point);
                StartCoroutine(explosionManager.ExplosionAtPoint(hit.point));

                if (hit.transform.gameObject.GetComponent<PhotonView>() != null)
                {
                    BombManager.instance.PushTarget(hit.transform.gameObject.GetComponent<PhotonView>().ViewID,
                        ShotgunForce, transform.position, explosionManager.explosionRadius);
                }

                TrailRenderer trail = Instantiate(bulletTrail, adjustedOrigin, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, adjustedOrigin, spreadDirection, shotgunRange, hit));
            }
            else
            {
                TrailRenderer trail = Instantiate(bulletTrail, adjustedOrigin, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, adjustedOrigin, spreadDirection, shotgunRange));
            }
        }

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
    }

    IEnumerator ShotgunCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootShotgun = true;
        shotgunAnimator.ResetTrigger("shot");
    }

    [PunRPC]
    void SpawnTrailRPC(Vector3 pos, Quaternion rot)
    {
        TrailRenderer trail = Instantiate(bulletTrail, pos, rot);
        Destroy(trail, trail.time);
    }


}
