using System;
using UnityEngine;

namespace DefaultNamespace.PowerUps
{
    public enum GravityAmmoType
    {
        Down,
        Up,
        Forward,
        Backward,
        Left,
        Right
    }

    public class GravityChangePowerup : PowerUpBase
    {
        [SerializeField] private GravityAmmoType ammoType;

        protected override void ApplyEffect(PlayerMovement player)
        {
            //player.SetCurrentGravityAmmoType(ammoType);
        }
        protected override void RemoveEffect(PlayerMovement player)
        {
            Debug.Log("");
        }
    }
}
