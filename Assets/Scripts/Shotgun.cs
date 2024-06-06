using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.VFX;
using DefaultNamespace;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private Transform shotgunBulletExit;
    [SerializeField] private float shotgunRange;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private Animator shotgunAnimator;
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private VisualEffect muzzleFlashVFX;

    [SerializeField] private int shotgunPellets = 5;
    [SerializeField] private float shotgunSpreadAngle = 10f;

    private ExplosionManager explosionManager;
    private PhotonView photonView;

    public float reloadSpeed = 1f;
    public float ShotgunForce = 400f;
    public bool canShootShotgun = true;
    public bool IsSecondaryGun = false;

    private void Awake()
    {
        explosionManager = GetComponent<ExplosionManager>();
        photonView = GetComponent<PhotonView>();
    }

    public void Shoot()
    {
        if (!canShootShotgun && !IsSecondaryGun)
            return;

        RaycastHit hit;
        Vector3 rayDirection = shotgunBulletExit.transform.forward;
        Vector3 adjustedOrigin = shotgunBulletExit.transform.position + rayDirection * 0.1f;

        //photonView.RPC("SpawnMuzzleFlash", RpcTarget.All);

        for (int i = 0; i < shotgunPellets; i++)
        {
            Vector3 spreadDirection = Quaternion.Euler(RandomSystem.GetGaussianRandomFloat(-shotgunSpreadAngle, shotgunSpreadAngle),
                RandomSystem.GetGaussianRandomFloat(-shotgunSpreadAngle, shotgunSpreadAngle), RandomSystem.GetGaussianRandomFloat(-shotgunSpreadAngle, shotgunSpreadAngle)) * rayDirection;

            Debug.DrawRay(adjustedOrigin, spreadDirection * shotgunRange, Color.red, 3);

            if (Physics.Raycast(adjustedOrigin, spreadDirection, out hit, shotgunRange))
            {
                Debug.DrawRay(adjustedOrigin, spreadDirection * shotgunRange, Color.green, 3);
                Vector3 sparkScale = new Vector3(0.04f, 0.04f, 0.04f);

                if (hit.transform.gameObject.GetComponent<PhotonView>() != null)
                {
                    BombManager.instance.PushTarget(hit.transform.gameObject.GetComponent<PhotonView>().ViewID,
                        ShotgunForce * 20f, transform.position, explosionManager.explosionRadius);

                    explosionManager.view.RPC("SpawnSparks", RpcTarget.All, hit.point, sparkScale);
                }
                else
                {
                    explosionManager.view.RPC("triggerEffectRPC", RpcTarget.All, hit.point, new Vector3(0.2f, 0.2f, 0.2f));
                    StartCoroutine(explosionManager.ExplosionAtPoint(hit.point));
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
        StartCoroutine(ShotgunCooldown(reloadSpeed));
        shotgunAnimator.SetTrigger("shot");
    }

    private void Update()
    {
        shotgun.SetActive(IsSecondaryGun);
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
        Destroy(trail.gameObject, trail.time);
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
    
    [PunRPC]
    private void SpawnMuzzleFlash()
    {
        Vector3 vfxPosition = shotgunBulletExit.position;  
        Quaternion vfxRotation = Quaternion.LookRotation(-shotgunBulletExit.forward); 

        VisualEffect muzzleFlashInstance = Instantiate(muzzleFlashVFX, vfxPosition, vfxRotation);
        muzzleFlashInstance.Play(); 
    }

   

}
