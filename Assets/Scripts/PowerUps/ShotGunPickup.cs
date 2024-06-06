using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunPickup : WeaponPickUpBase
{
    private Shotgun shotgun;

    protected override Weapon GetWeapon(PlayerMovement player)
    {
        return player.GetComponentInChildren<Shotgun>();
    }

    protected override void GiveWeapon(PlayerMovement player)
    {
        shotgun = player.GetComponentInChildren<Shotgun>();
        shotgun.ActivateAsSecondary();
    }

    protected override void RemoveWeapon(PlayerMovement player)
    {
        shotgun.DeactivateAsSecondary();
    }
}
