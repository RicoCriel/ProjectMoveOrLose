using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Canon canon;
    [SerializeField] private Shotgun shotGun;
    [SerializeField] private ExplosionManager explosionManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraView;

    private Rigidbody playerRb;

    private bool isFiringShotgun;
    private bool isFiringCanon;

    private void Start()
    {
        playerRb = playerMovement.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleShootingInput();
    }

    private void FixedUpdate()
    {
        if(isFiringShotgun == true)
        {
            FireWeapon(shotGun.ShotgunForce);
            isFiringShotgun = false;
            playerMovement.gravityIncreaseRate = 1.5f;
        }

        if(isFiringCanon == true)
        {
            FireWeapon(canon.CanonForce);
            isFiringCanon = false;
            playerMovement.gravityIncreaseRate = 1.5f;
        }
    }

    private void HandleShootingInput()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            HandleCanon();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            HandleShotGun();
        }
    }

    private void HandleShotGun()
    {
        if (shotGun.canShootShotgun)
        {
            isFiringShotgun = true;
        }
        shotGun.Shoot();
    }

    private void HandleCanon()
    {
        if (canon.canShootCanon)
        {
            isFiringCanon = true;
        }
        canon.Shoot(ref cameraView, this.gameObject);
    }

    private void FireWeapon(float force)
    {
        explosionManager.AddPush(-cameraView.transform.forward, force, playerRb);
        playerMovement.gravityIncreaseRate = 10f;
        Debug.Log(playerMovement.gravityIncreaseRate);
    }
}
