using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunPickup : WeaponPickUpBase
{
    private Shotgun shotgun;
    protected override void GiveWeapon(PlayerMovement player)
    {
        shotgun = player.GetComponentInChildren<Shotgun>();
        shotgun.IsSecondaryGun = true;
    }

    protected override void RemoveWeapon(PlayerMovement player)
    {
        shotgun.IsSecondaryGun = false;
    }
}
