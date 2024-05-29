using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField] public float explosionRadius;
    public float radiusDestroyMultiplier = 1.5f;
    [SerializeField] private float explosionForce = 10f;

    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 10;

    [SerializeField] GameObject explosionEffect;
    [SerializeField] GameObject sparkEffect;

    public PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public IEnumerator ExplosionAtPoint(Vector3 explosionPoint)
    {
        Collider[] blockColliders = Physics.OverlapSphere(explosionPoint, explosionRadius);
        foreach (var blockCollider in blockColliders)
        {
            if (blockCollider.CompareTag("Block"))
            {
                float distance = Vector3.Distance(explosionPoint, blockCollider.transform.position);
                int calculatedDamage = CalculateDamage(distance);

                MapGenerator.instance.DamageBlock(blockCollider.transform.position, calculatedDamage);
                MapGenerator.instance.SetRoomDirty();
            }
        }

        yield return null;
    }

    private int CalculateDamage(float distance)
    {
        int damage = Mathf.CeilToInt(Mathf.Lerp(maxDamage, minDamage, distance));
        return damage;
    }

    public void AddExplosionForce(ref Vector3 impact, ref Vector3 playerVelocity)
    {
        if (impact.magnitude > 0.2F)
        {
            playerVelocity += impact * Time.deltaTime;
        }

        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 15 * Time.deltaTime);
    }

    public void AddImpact(Vector3 explosionOrigin, float force, Vector3 playerVelocity, Vector3 impact)
    {
        Vector3 dir = this.transform.position - explosionOrigin;
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground

        // Subtract player's current velocity from the impact force
        Vector3 adjustedImpact = dir.normalized * force / 3 - playerVelocity;

        // Add the adjusted impact to the current impact
        impact += adjustedImpact;
    }

    //public void AddPush(Vector3 direction, float force, Vector3 playerVelocity, ref Vector3 impact)
    //{
    //    Vector3 dir = direction;
    //    // Subtract player's current velocity from the impact force
    //    Vector3 adjustedImpact = dir.normalized * force / 3 - playerVelocity;
    //    // Add the adjusted impact to the current impact
    //    impact += adjustedImpact;
    //    Debug.Log("Add push executed");
    //}

    public void AddPush(Vector3 direction, float force, Rigidbody playerRb)
    {
        // Apply the directional push force to the player's Rigidbody
        Vector3 forceVector = direction.normalized * force;
        playerRb.AddForce(forceVector, ForceMode.Impulse);

        Debug.DrawLine(playerRb.position, playerRb.position + forceVector, Color.red, 5f);
    }

    [PunRPC]
    void triggerEffectRPC(Vector3 pos, Vector3 scale)
    {
        GameObject effect = Instantiate(explosionEffect, pos, Quaternion.identity);
        effect.transform.localScale = scale;
        Destroy(effect, 2f);
    }
    
    [PunRPC]
    void triggerEffectRPC(Vector3 pos)
    {
        GameObject effect = Instantiate(explosionEffect, pos, Quaternion.identity);

        Destroy(effect, 2f);
    }

    [PunRPC]
    private void SpawnSparks(Vector3 pos, Vector3 scale)
    {
        GameObject spark = Instantiate(sparkEffect, pos, Quaternion.identity);
        spark.transform.localScale = scale;
    }

}
