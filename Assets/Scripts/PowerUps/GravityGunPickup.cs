using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGunPickup : WeaponPickUpBase
{
    private GravityGun gravityGun;
    protected override void GiveWeapon(PlayerMovement player)
    {
        gravityGun = player.GetComponentInChildren<GravityGun>();
        gravityGun.IsSecondaryGun = true;
    }

    protected override void RemoveWeapon(PlayerMovement player)
    {
        gravityGun.IsSecondaryGun = false;
    }

}
