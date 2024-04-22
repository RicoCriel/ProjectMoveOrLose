using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
   public GameObject playerPrefab;
   public GameObject Tiles;

   public float minX;
   public float maxX;
   public float minY;
   public float maxY;


   private void Start()
   {
     
      
      Vector2 randomPosition = new Vector3(UnityEngine.Random.Range(minX, maxX),4, UnityEngine.Random.Range(minY, maxY));
      PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
   }
}
