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
    [SerializeField] private VisualEffect muzzleFlashVFX;
    [SerializeField] private float RocketBulletSpeed;
    public float ReloadSpeed = 1f;
    public float AnimationSpeed;

    [SerializeField] private Animator canonAnimator;

    public float CanonForce = 600f;
    public bool canShootCanon = true;
    public bool IsCanonShooting = false;
    public float rocketJumpForce;

    [SerializeField] private GameObject player;
    private PhotonView photonView;


    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        AnimationSpeed = canonAnimator.speed;
    }

    public void Shoot(ref Transform playerView)
    {
        if (canShootCanon)
        {
            IsCanonShooting = true;
            photonView.RPC("SpawnMuzzleFlash", RpcTarget.All);
            GameObject bullet = PhotonNetwork.Instantiate(rocketBullet.name, rocketBulletExit.transform.position, rocketBulletExit.transform.rotation);
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            bullet.GetComponent<Rocket>().view = bullet.GetComponent<PhotonView>();
            bullet.GetComponent<Rocket>().player = player;

            rbBullet.velocity = playerView.transform.forward * RocketBulletSpeed;
            canonAnimator.SetTrigger("Shot");

            canShootCanon = false;
            StartCoroutine(CanonCooldown(ReloadSpeed));
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

        VisualEffect muzzleFlashInstance = Instantiate(muzzleFlashVFX, vfxPosition, vfxRotation);
        muzzleFlashInstance.Play(); 
    }
}
