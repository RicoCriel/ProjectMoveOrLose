using DefaultNamespace.PowerUps;
using Photon.Pun;
using System.Collections;
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

    public bool canShootGravityGun = true;
    public bool IsGravityGunShooting = false;

    private PhotonView photonView;
    private Coroutine countdown;
    private GravityAmmoType currentAmmoType;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void Shoot(ref Transform playerView, GameObject playerObject, float speedMultiplier)
    {
        if (canShootGravityGun)
        {
            IsGravityGunShooting = true;

            Vector3 bulletStartPosition = gravityBulletExit.position + playerView.forward; 

            GameObject bullet = PhotonNetwork.Instantiate(gravityBullet.name, bulletStartPosition, playerView.rotation);

            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.velocity = playerView.forward * gravityBulletSpeed * speedMultiplier;

            gravityGunAnimator.SetTrigger("Shot");
            canShootGravityGun = false;

            if (countdown != null)
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
