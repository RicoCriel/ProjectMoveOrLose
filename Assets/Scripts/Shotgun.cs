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
    private PhotonView photonView;

    private float shotgunCountdown = 1f;

    public float ShotgunDirectionSpeed = 70f;
    public float ShotgunForce =  400f;
    public bool canShootShotgun = true;

    private void Awake()
    {
        explosionManager = GetComponent<ExplosionManager>();
        photonView = GetComponent<PhotonView>();
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

                photonView.RPC("SpawnTrail", RpcTarget.All, adjustedOrigin, hit.point);
            }
            else
            {
                Vector3 endPos = adjustedOrigin + spreadDirection * shotgunRange;
                photonView.RPC("SpawnTrail", RpcTarget.All, adjustedOrigin, endPos);
            }
        }

        canShootShotgun = false;
        StartCoroutine(ShotgunCooldown(shotgunCountdown));
        shotgunAnimator.SetTrigger("shot");
    }

    private IEnumerator SpawnTrailCoroutine(TrailRenderer trail, Vector3 startPos, Vector3 endPos)
    {
        float time = 0;

        while (time < trail.time)
        {
            trail.transform.position = Vector3.Lerp(startPos, endPos, time / trail.time);
            time += Time.deltaTime;
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
    private void SpawnTrail(Vector3 startPos, Vector3 endPos)
    {
        TrailRenderer trail = Instantiate(bulletTrail, startPos, Quaternion.identity);
        StartCoroutine(SpawnTrailCoroutine(trail, startPos, endPos));
    }
}
