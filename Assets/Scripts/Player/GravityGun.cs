using DefaultNamespace.PowerUps;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GravityGun : MonoBehaviour
{
    [SerializeField] private GameObject gravityBullet;
    [SerializeField] private Transform gravityBulletExit;
    [SerializeField] private VisualEffect muzzleFlashVFX;
    [SerializeField] private float gravityBulletSpeed;
    [SerializeField] private float gravityGunCountDown;

    [SerializeField] private Animator gravityGunAnimator;
    [SerializeField] private PlayerMovement playerMovement;

    public float GravityGunForce = 600f;
    public bool canShootGravityGun = true;
    public bool IsGravityGunShooting = false;
    public float rocketJumpForce;

    private PhotonView photonView;
    private Coroutine countdown;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void Shoot(ref Transform playerView, GameObject playerObject, float speedMultiplier)
    {
        if(canShootGravityGun)
        {
            IsGravityGunShooting = true;

            //GravityAmmoType currentAmmoType = playerMovement.GetCurrentGravityAmmoType();

            GameObject bullet = /*PhotonNetwork.*/Instantiate(gravityBullet, gravityBulletExit.transform.position, gravityBulletExit.transform.rotation);
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();

            //bullet.GetComponent<Rocket>().view = bullet.GetComponent<PhotonView>();
            //bullet.GetComponent<Rocket>().player = playerObject;

            rbBullet.velocity = playerView.transform.forward * gravityBulletSpeed * speedMultiplier;


            gravityGunAnimator.SetTrigger("Shot");
            canShootGravityGun = false;

            if(countdown != null)
                StopCoroutine(countdown);
            
            countdown = StartCoroutine(GravityGunCooldown(gravityGunCountDown));
        }
    }

    IEnumerator GravityGunCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootGravityGun = true;
        gravityGunAnimator.ResetTrigger("Shot");
        IsGravityGunShooting = false;
    }
}
