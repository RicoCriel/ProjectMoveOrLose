using DefaultNamespace;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    [SerializeField] private GameObject rocketBullet;
    [SerializeField] private Transform rocketBulletExit;
    [SerializeField] private float RocketBulletSpeed;
    [SerializeField] private float CanonCountDown;

    [SerializeField] private Animator canonAnimator;

    public float CanonDirectionSpeed = 100f;
    public float CanonForce = 600f;
    public bool canShootCanon = true;
    public bool IsCanonShooting = false;
    public float rocketJumpForce;

    public void Shoot(ref Transform playerView, GameObject playerObject)
    {
        if (canShootCanon)
        {
            IsCanonShooting = true;
            GameObject bullet = PhotonNetwork.Instantiate(rocketBullet.name, rocketBulletExit.transform.position, rocketBulletExit.transform.rotation);
            Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();
            bullet.GetComponent<Rocket>().view = bullet.GetComponent<PhotonView>();
            bullet.GetComponent<Rocket>().player = playerObject;

            rbBullet.velocity = playerView.transform.forward * RocketBulletSpeed;
            canonAnimator.SetTrigger("Shot");

            canShootCanon = false;
            StartCoroutine(CanonCooldown(CanonCountDown));
        }
    }

    IEnumerator CanonCooldown(float cd)
    {
        yield return new WaitForSeconds(cd);
        canShootCanon = true;
        canonAnimator.ResetTrigger("Shot");
        IsCanonShooting = false;
    }
}