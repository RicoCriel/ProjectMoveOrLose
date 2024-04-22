using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerControllerTest : MonoBehaviour
{
   PhotonView photonView;

   [SerializeField] private GameObject bulletPrefab;
   [SerializeField] private Transform firePoint;
   [SerializeField] private float bulletSpeed = 10f;
   [SerializeField] private string localBulletTag = "LocalBullet";
   
   [SerializeField]
   private float moveSpeed = 5f;

   private void Start()
   {
      if (photonView == null)
      {
         photonView = GetComponent<PhotonView>();
      }
   }

   private void Update()
   {
      if (photonView.IsMine)
      {
         ProcessInputs();
      }
   }
   private void ProcessInputs()
   {
      float moveX = Input.GetAxis("Horizontal");
      float moveZ = Input.GetAxis("Vertical");

      Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
      transform.Translate(movement);
      
      if (Input.GetKeyDown(KeyCode.Space))
      {
         photonView.RPC("SpawnBullet", RpcTarget.AllViaServer, firePoint.position, firePoint.rotation);
      }
   }
   
   [PunRPC]
   private void SpawnBullet(Vector3 position, Quaternion rotation)
   {
      GameObject bullet = Instantiate(bulletPrefab, position, rotation);
      // bullet.tag = localBulletTag;

      bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;

      // Destroy the bullet after a certain time to prevent cluttering the scene
      Destroy(bullet, 5f);
   }
   
   private void OnTriggerEnter(Collider other)
   {
      Destroy(other.gameObject);
   }
}
