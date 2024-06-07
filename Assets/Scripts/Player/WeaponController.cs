using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using DG.Tweening;

public class WeaponController : MonoBehaviour
{
    public Canon canon;
    public GravityGun gravityGun;
    public Shotgun shotgun;

    public ExplosionManager explosionManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraView;
    [SerializeField] private GameObject player;
    
    private PhotonView view;
    private Rigidbody playerRb;

    private bool isFiringGravityGun;
    private bool isFiringCanon;
    private bool cannonOnCooldown = false;

    public float gravityGunChargeTime;
    private const float maxChargeTime = 5.0f;
    private const float minChargeTime = 0.25f; 
    private float originalGravityIncreaseRate;
    private float currentGravityIncreaseRate;
    private float smoothTime = 1f;

    [SerializeField]
    private GameObject canvasPrefab; 
    private GravityGunChargeUI gravityGunChargeUI;
    private CanonUI canonUI;

    public Weapon currentSecondaryWeapon { get; private set; }

    private void Start()
    {
        playerRb = playerMovement.GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        originalGravityIncreaseRate = playerMovement.gravityIncreaseRate;
        currentGravityIncreaseRate = originalGravityIncreaseRate;

        if (canvasPrefab != null)
        {
            GameObject gunCanvas = Instantiate(canvasPrefab, player.transform);
            gravityGunChargeUI = gunCanvas.GetComponentInChildren<GravityGunChargeUI>();
            canonUI = gunCanvas.GetComponentInChildren<CanonUI>();
        }
    }

    private void Update()
    {
        if (view.IsMine)
        {
            HandleShootingInput();
            HandleGravityGunCharging();
            gravityGunChargeUI.ChargeTime = gravityGunChargeTime;
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

    public void DeactivateCurrentSecondaryWeapon()
    {
        if (currentSecondaryWeapon != null)
        {
            currentSecondaryWeapon.DeactivateAsSecondary();
            currentSecondaryWeapon = null;
        }
    }

    public void SetActiveSecondaryWeapon(Weapon weapon)
    {
        if (currentSecondaryWeapon == null)
        {
            currentSecondaryWeapon = weapon;
            weapon.ActivateAsSecondary();
        }
        else
        {
            Debug.Log("A secondary weapon is already active. Cannot set a new one.");
        }
    }

    public void RemoveActiveSecondaryWeapon(Weapon weapon)
    {
        if (currentSecondaryWeapon == weapon)
        {
            currentSecondaryWeapon.DeactivateAsSecondary();
            currentSecondaryWeapon = null;
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
            HandleShotGun();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            HandleGravityGun();
            gravityGunChargeUI.ResetFillAmount();
        }
    }

    private void HandleGravityGunCharging()
    {
        if (Input.GetButton("Fire1") && currentSecondaryWeapon == gravityGun)
        {
            gravityGunChargeTime += Time.deltaTime;
            gravityGunChargeTime = Mathf.Min(gravityGunChargeTime, maxChargeTime);
        }
    }

    private void HandleGravityGun()
    {
        if (!currentSecondaryWeapon == gravityGun)
            return;

        if (gravityGunChargeTime >= minChargeTime && gravityGun.canShootGravityGun)
        {
            isFiringGravityGun = true;
            gravityGun.Shoot(ref cameraView, gravityGunChargeTime);
        }
    }

    private void HandleShotGun()
    {
        if (!currentSecondaryWeapon == shotgun)
            return;

        shotgun.canShootShotgun = true;
        shotgun.Shoot();
    }

    private void HandleCanon()
    {
        if (canon.canShootCanon)
        {
            isFiringCanon = true;
            explosionManager.AddPush(-cameraView.forward, canon.CanonForce, playerRb);
            playerMovement.gravityIncreaseRate = 20f;

            if (!cannonOnCooldown)
            {
                cannonOnCooldown = true;
                canonUI.StartFading();
                StartCoroutine(CanonCooldown());
            }
        }
        canon.Shoot(ref cameraView);
    }

    private void FireWeapon(float force)
    {
        explosionManager.AddPush(-cameraView.transform.forward, force, playerRb);
    }

    private void FireGravityGun()
    {
        gravityGun.Shoot(ref cameraView, gravityGunChargeTime);
        gravityGunChargeTime = 0f;
        gravityGunChargeUI.ResetFillAmount();
    }

    private void HandleGravityStrength()
    {
        if (!isFiringCanon)
        {
            playerMovement.gravityIncreaseRate = Mathf.Lerp(playerMovement.gravityIncreaseRate,
                originalGravityIncreaseRate, Time.deltaTime * smoothTime);
        }
    }

    private IEnumerator CanonCooldown()
    {
        yield return new WaitForSeconds(canon.ReloadSpeed); 
        cannonOnCooldown = false;
        canonUI.StopFading();
    }
}