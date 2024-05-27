using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Canon canon;
    [SerializeField] private GravityGun gravityGun;
    [SerializeField] private ExplosionManager explosionManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraView;
    [SerializeField] private GameObject player;
    
    private PhotonView view;
    private Rigidbody playerRb;

    private bool isFiringGravityGun;
    private bool isFiringCanon;
    private bool cannonOnCooldown = false;

    private float gravityGunChargeTime;
    private const float maxChargeTime = 5.0f;
    private const float minChargeTime = 0.25f; 
    private float originalGravityIncreaseRate;
    private float currentGravityIncreaseRate;
    private float cannonCooldownDuration = 0.5f;
    private float cannonCooldownTimer = 0.0f;
    private float smoothTime = 1f;

    private void Start()
    {
        playerRb = playerMovement.GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        originalGravityIncreaseRate = playerMovement.gravityIncreaseRate;
        currentGravityIncreaseRate = originalGravityIncreaseRate;
    }

    private void Update()
    {
        if (view.IsMine)
        {
            HandleShootingInput();
            HandleGravityGunCharging();
            HandleCanonCooldown();
            HandleGravityStrength();
        }
    }

    private void FixedUpdate()
    {
        if (isFiringGravityGun)
        {
            FireGravityGun();
            isFiringGravityGun = false;
        }

        if (isFiringCanon)
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
        if (gravityGunChargeTime >= minChargeTime && gravityGun.canShootGravityGun)
        {
            isFiringGravityGun = true;
            gravityGun.Shoot(ref cameraView, player, gravityGunChargeTime);
        }
    }

    private void HandleCanon()
    {
        if (!cannonOnCooldown && canon.canShootCanon)
        {
            isFiringCanon = true;
            explosionManager.AddPush(-cameraView.forward, canon.CanonForce, playerRb);
            playerMovement.gravityIncreaseRate = 20f;

            cannonOnCooldown = true;
            cannonCooldownTimer = cannonCooldownDuration;
        }
        canon.Shoot(ref cameraView, this.gameObject);
    }

    private void HandleCanonCooldown()
    {
        if (cannonOnCooldown)
        {
            cannonCooldownTimer -= Time.deltaTime;
            if (cannonCooldownTimer <= 0.0f)
            {
                cannonOnCooldown = false;
            }
        }
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

    private void HandleGravityStrength()
    {
        if (!isFiringCanon)
        {
            playerMovement.gravityIncreaseRate = Mathf.Lerp(playerMovement.gravityIncreaseRate,
                originalGravityIncreaseRate, Time.deltaTime * smoothTime);
        }
    }
}