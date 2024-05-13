using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamSetup : MonoBehaviour
{
    private GameObject playerMainCamera;
    private GameObject playerGunCamera;
    private List<GameObject> playerGuns;
    public int playerID;

    public void SetupCamLayers()
    {
        playerMainCamera = this.gameObject;
        playerGunCamera = this.transform.GetChild(0).gameObject;

        playerMainCamera.tag = "MainCamera" + playerID;
        playerGunCamera.tag = "WeaponCamera" + playerID;

        foreach (var playerGun in playerGuns)
        {
            playerGun.layer = LayerMask.NameToLayer("Weapon" + playerID);
            playerGun.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Weapon" + playerID);
        }

        playerMainCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Weapon" + playerID));
        playerGunCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("Weapon" + playerID);
    }

    public void AddGun(GameObject gun)
    {
        playerGuns.Add(gun);
    }
}

