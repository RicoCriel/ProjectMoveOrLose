using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public int playerOwner;
    private Vector3 explosionOrigin;
    public float explosionRadius = 10f;
    public float explosionForce = 200f;

    

    

    private void OnTriggerEnter(Collider other)
    {
        explosionOrigin = transform.position;
        Debug.Log("trigger enter");
        if (other.GetComponent<PhotonView>())
        {
            if (other.GetComponent<PhotonView>().ViewID == playerOwner)
            {
            
            }
            else
            {
                Debug.Log("activate");
                Collider[] objectsInRange = Physics.OverlapSphere(explosionOrigin, explosionRadius);
         
                foreach (Collider collision in objectsInRange)
                {
                    QuakeCharController player = collision.GetComponent<QuakeCharController>();
                    if (player != null)
                    {
                        // Apply knockback force to player if they are in explosion radius
                        player.AddImpact(explosionOrigin, explosionForce);
                    }
                }
                Destroy(this.gameObject); 
            
            }
        }
        else
        {
            Debug.Log("activate");
            Collider[] objectsInRange = Physics.OverlapSphere(explosionOrigin, explosionRadius);
         
            foreach (Collider collision in objectsInRange)
            {
                QuakeCharController player = collision.GetComponent<QuakeCharController>();
                if (player != null)
                {
                    // Apply knockback force to player if they are in explosion radius
                    player.AddImpact(explosionOrigin, explosionForce);
                }
            }
            Destroy(this.gameObject); 
            
        }
        
            
        
    }
}
