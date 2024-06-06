using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunPickup : WeaponPickUpBase
{
    //private MachineGun machineGun;
    protected override void GiveWeapon(PlayerMovement player)
    {
        //machineGun = player.GetComponentInChildren<MachineGun>();
        //machineGunPickup.IsSecondaryGun = true;
        throw new System.NotImplementedException();
    }

    protected override void RemoveWeapon(PlayerMovement player)
    {
        //machineGun = player.GetComponentInChildren<MachineGun>();
        //machineGunPickup.IsSecondaryGun = false;
        throw new System.NotImplementedException();
    }
}
