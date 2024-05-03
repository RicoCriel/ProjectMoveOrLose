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

    public IEnumerator ExplosionAtPoint(Vector3 explosionPoint)
    {
        // OverlapSphere to find colliders within the explosion radius
        Collider[] playerColliders = Physics.OverlapSphere(explosionPoint, explosionRadius * radiusDestroyMultiplier);
        foreach (var playerCollider in playerColliders)
        {
            if (playerCollider.CompareTag("Player"))
            {
                //Debug.Log("Pushing Player with id" + playerCollider.GetComponent<PhotonView>().ViewID);
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
}
