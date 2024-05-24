using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Canon canon;
    [SerializeField] private GravityGun gravityGun;
    [SerializeField] private ExplosionManager explosionManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraView;
    [SerializeField] private GameObject player;

    private Rigidbody playerRb;

    private bool isFiringGravityGun;
    private bool isFiringCanon;
    private float gravityGunChargeTime;
    private const float maxChargeTime = 3.0f;

    private void Start()
    {
        playerRb = playerMovement.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleShootingInput();
        HandleGravityGunCharging();
    }

    private void FixedUpdate()
    {
        if (isFiringGravityGun)
        {
            isFiringGravityGun = false;
            FireGravityGun();
        }

        if (isFiringCanon == true)
        {
            FireWeapon(canon.CanonForce);
            isFiringCanon = false;
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
            gravityGunChargeTime = 0f;  
        }

        if (Input.GetButtonUp("Fire1"))
        {
            HandleGravityGun();
        }
    }

    private void HandleGravityGunCharging()
    {
        if (Input.GetButton("Fire1"))
        {
            gravityGunChargeTime += Time.deltaTime;
            gravityGunChargeTime = Mathf.Min(gravityGunChargeTime, maxChargeTime); 
        }
    }

    private void HandleGravityGun()
    {
        if(gravityGun.canShootGravityGun)
        {
            isFiringGravityGun =true;
        }
        gravityGun.Shoot(ref cameraView, player, gravityGunChargeTime);
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
    }

    private void FireGravityGun()
    {
        gravityGun.Shoot(ref cameraView, this.gameObject, gravityGunChargeTime);
        gravityGunChargeTime = 0f;  
    }
}
