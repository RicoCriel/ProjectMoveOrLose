using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private Vector3 explosionOrigin;
    public float explosionRadius = 3f;
    public float explosionForce = 70f;

    private void Start()
    {
        explosionOrigin = transform.position;

        
    }

    private void OnCollisionEnter(Collision other)
    {
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
        Destroy(this);
    }
}
