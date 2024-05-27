using DefaultNamespace.PowerUps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
    public GravityAmmoType ammotype;
    private Dictionary<GravityAmmoType, PlayerMovement.GravityState> gravityEffectMapping;

    private void Start()
    {
        Destroy(this.gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            InitializeGravityEffectMapping();

            if (gravityEffectMapping.TryGetValue(ammotype, out var newGravityState))
            {
                playerMovement.SetGravityState(newGravityState);
            }
        }
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
