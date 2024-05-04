using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DefaultNamespace;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField] private float explosionRadius;
    [SerializeField] private float radiusDestroyMultiplier = 1.5f;
    [SerializeField] private float explosionForce = 10f;

    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 10;

    [SerializeField] GameObject explosionEffect;
    public PhotonView view;

    public IEnumerator ExplosionAtPoint(Vector3 explosionPoint)
    {
        // OverlapSphere to find colliders within the explosion radius
        Collider[] playerColliders = Physics.OverlapSphere(explosionPoint, explosionRadius * radiusDestroyMultiplier);
        foreach (var playerCollider in playerColliders)
        {
            if (playerCollider.CompareTag("Player"))
            {
                Debug.Log("Pushing Player with id" + playerCollider.GetComponent<PhotonView>().ViewID);
                BombManager.instance.PushTarget(playerCollider.GetComponent<PhotonView>().ViewID, explosionForce, explosionPoint, explosionRadius);
            }
        }

        Collider[] blockColliders = Physics.OverlapSphere(explosionPoint, explosionRadius /** radiusDestroyMultiplier*/);
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

        // Return control to the caller
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

    public void AddPush(Vector3 direction, float force, Vector3 playerVelocity, ref Vector3 impact)
    {
        Vector3 dir = direction;
        // Subtract player's current velocity from the impact force
        Vector3 adjustedImpact = dir.normalized * force / 3 - playerVelocity;
        // Add the adjusted impact to the current impact
        impact += adjustedImpact;
    }

    [PunRPC]
    void triggerEffectRPC(Vector3 pos)
    {
        GameObject effect = Instantiate(explosionEffect, pos, Quaternion.identity);
        Destroy(effect, 2f);
    }
}
