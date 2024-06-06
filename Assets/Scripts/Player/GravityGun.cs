using DefaultNamespace.PowerUps;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class GravityGun : Weapon
{
    [SerializeField] private GameObject gravityBullet;
    [SerializeField] private GameObject gravityGun;
    [SerializeField] private Transform gravityBulletExit;
    [SerializeField] private VisualEffect muzzleFlashVFX;
    [SerializeField] private float gravityBulletSpeed;
    [SerializeField] private float gravityGunCountDown;

    [SerializeField] private Animator gravityGunAnimator;
    [SerializeField] private PlayerMovement playerMovement;

    public bool canShootGravityGun = true;
    public bool IsGravityGunShooting = false;
    private Coroutine countdown;
    private GravityAmmoType currentAmmoType;

    public void Shoot(ref Transform playerView, float speedMultiplier)
    {
        if (!canShootGravityGun || !IsSecondaryGun)
        {
            return;
        }
        
        IsGravityGunShooting = true;

        Vector3 bulletStartPosition = gravityBulletExit.position + playerView.forward; 

        GameObject bullet = PhotonNetwork.Instantiate(gravityBullet.name, bulletStartPosition, playerView.rotation);
        bullet.GetComponent<GravityProjectile>().player = playerMovement.gameObject;
        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
        rbBullet.velocity = playerView.forward * gravityBulletSpeed * speedMultiplier;

        gravityGunAnimator.SetTrigger("Shot");
        canShootGravityGun = false;

        if (countdown != null)
            StopCoroutine(countdown);

        countdown = StartCoroutine(GravityGunCooldown(gravityGunCountDown));
    }

    IEnumerator GravityGunCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootGravityGun = true;
        gravityGunAnimator.ResetTrigger("Shot");
        IsGravityGunShooting = false;
    }

    protected override void OnActivate()
    {
        gravityGun.SetActive(true);
    }

    protected override void OnDeactivate()
    {
        gravityGun.SetActive(false);
    }
}
