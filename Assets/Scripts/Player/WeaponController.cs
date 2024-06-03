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
            HandleGravityStrength();
            gravityGunChargeUI.ChargeTime = gravityGunChargeTime;
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
            gravityGun.Shoot(ref cameraView, gravityGunChargeTime);
        }
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