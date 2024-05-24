using DefaultNamespace.PowerUps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GravityEffect
{
    Down,
    Up,
    Forward,
    Backward,
    Left,
    Right
}

public class GravityProjectTile : MonoBehaviour
{
    private Rigidbody rb;
    private GravityAmmoType gravityEffect;
    private Dictionary<GravityAmmoType, PlayerMovement.GravityState> gravityEffectMapping;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        InitializeGravityEffectMapping();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            Debug.Log(playerMovement.currentGravityState);
            playerMovement.SetGravityState(PlayerMovement.GravityState.Up);


        }

        //if (playerMovement != null && gravityEffectMapping.TryGetValue(gravityEffect, out var newGravityState))
        //{
        //    playerMovement.SetGravityState(newGravityState);
        //    Debug.Log("Player gravity state set to: " + playerMovement.currentGravityState);
        //}
    }

    public void SetGravityEffect(GravityAmmoType ammoType)
    {
        gravityEffect = ammoType;
    }

    private void InitializeGravityEffectMapping()
    {
        gravityEffectMapping = new Dictionary<GravityAmmoType, PlayerMovement.GravityState>
        {
            { GravityAmmoType.Down, PlayerMovement.GravityState.Down },
            { GravityAmmoType.Up, PlayerMovement.GravityState.Up },
            { GravityAmmoType.Forward, PlayerMovement.GravityState.Forward },
            { GravityAmmoType.Backward, PlayerMovement.GravityState.Backward },
            { GravityAmmoType.Left, PlayerMovement.GravityState.Left },
            { GravityAmmoType.Right, PlayerMovement.GravityState.Right }
        };
    }
}
