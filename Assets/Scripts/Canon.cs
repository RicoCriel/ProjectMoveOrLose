using DefaultNamespace;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Canon : MonoBehaviour
{
    [SerializeField] private GameObject rocketBullet;
    [SerializeField] private Transform rocketBulletExit;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float RocketBulletSpeed;
    public float ReloadSpeed = 1f;
    public float AnimationSpeed;

    [SerializeField] private Animator canonAnimator;

    public float CanonForce;
    public bool canShootCanon = true;
    public bool IsCanonShooting = false;

    [SerializeField] private GameObject player;
    public float RadiusMultiplier;
    private PhotonView photonView;
    private Coroutine canonCoolDown;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        AnimationSpeed = canonAnimator.speed;
        RadiusMultiplier = 1.5f;
    }

    public void Shoot(ref Transform playerView)
    {
        if (canShootCanon && photonView.IsMine)
        {
            IsCanonShooting = true;
            //photonView.RPC("SpawnMuzzleFlash", RpcTarget.All);
            GameObject bullet = PhotonNetwork.Instantiate(rocketBullet.name, rocketBulletExit.transform.position, rocketBulletExit.transform.rotation);
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            bullet.GetComponent<Rocket>().view = bullet.GetComponent<PhotonView>();
            bullet.GetComponent<Rocket>().player = player;
            bullet.GetComponent<Rocket>().radiusDestroyMultiplier = RadiusMultiplier;

            rbBullet.velocity = playerView.transform.forward * RocketBulletSpeed;
            canonAnimator.SetTrigger("Shot");

            canShootCanon = false;
            if(canonCoolDown != null)
            {
                StopCoroutine(canonCoolDown);
            }

            canonCoolDown = StartCoroutine(CanonCooldown(ReloadSpeed));
        }
    }

    IEnumerator CanonCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootCanon = true;
        canonAnimator.ResetTrigger("Shot");
        IsCanonShooting = false;
    }

    [PunRPC]
    private void SpawnMuzzleFlash()
    {
        Vector3 vfxPosition = rocketBulletExit.position; 
        Quaternion vfxRotation = Quaternion.LookRotation(-rocketBulletExit.forward);
        GameObject muzzleFlash = PhotonNetwork.Instantiate(muzzleFlashPrefab.name, vfxPosition, vfxRotation);
        VisualEffect muzzleFlashInstance = muzzleFlash.GetComponent<VisualEffect>();
        muzzleFlashInstance.Play();
    }
}
