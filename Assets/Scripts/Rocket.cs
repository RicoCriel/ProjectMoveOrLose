using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
namespace DefaultNamespace
{
    public class Rocket : MonoBehaviourPunCallbacks
    {
        [Header("explosion")]
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private bool destroyBlocks = true;
        [SerializeField] private float explosionForce = 1000f;
        [SerializeField] private float radiusDestroyMultiplier = 1.5f;

        [SerializeField] private int minDamage = 1; 
        [SerializeField] private int maxDamage = 10;

        [SerializeField] private float maxExplosionForce = 1000f;
        [SerializeField] private float minExplosionForce = 100f;

        public GameObject player;
        public PhotonView view;

        [Header("refs")]
        [SerializeField] GameObject explosionEffect;

        bool exploded = false;
        bool collHappened = false;
        Vector3 collisionpoint;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != player)
            {
              if (exploded) return;
                          collHappened = true;
                          collisionpoint = other.transform.position;
                          Explode();  
            }
        }

        public void Explode()
        {
            if (exploded) return;
            exploded = true;

            StartCoroutine(Explosion());
        }

        public IEnumerator Explosion()
        {
            bool posSet = false;
            Vector3 explosionPoint = new Vector3();

            while (!posSet)
            {
                if (collHappened)
                {
                    explosionPoint = collisionpoint;
                    posSet = true;
                }
                else
                {
                    explosionPoint = transform.position;
                    posSet = true;
                }

                yield return null;
            }

            
            view.RPC("triggerEffectRPC", RpcTarget.All, transform.position);
            
            
            Collider[] Playercolliders = Physics.OverlapSphere(explosionPoint, explosionRadius * radiusDestroyMultiplier);
            
            foreach (var hit in Playercolliders)
            {
                
                if (hit.tag == "Player")
                {
                    Debug.Log("Pushing Player with id" + hit.GetComponent<PhotonView>().ViewID);
                    BombManager.instance.PushTarget(hit.GetComponent<PhotonView>().ViewID, explosionForce, explosionPoint, explosionRadius);
                }
            }
            
            Collider[] BlockCollider = Physics.OverlapSphere(explosionPoint, explosionRadius );
            foreach (var hit in BlockCollider)
            {
                if (hit.tag == "Block")
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    int calculatedDamage = CalculateDamage(distance);

                    MapGenerator.instance.DamageBlock(hit.transform.position, calculatedDamage);
                    MapGenerator.instance.SetRoomDirty();
                }
            }
            BombManager.instance.DestroyBomb(view.ViewID);
        }

        public IEnumerator ExplosionAtPoint(Vector3 explosionPoint)
        {
            // Perform an explosion effect at the specified point
            //TriggerExplosionEffect(explosionPoint);

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

            // Return control to the caller
            yield return null;
        }

        private int CalculateDamage(float distance)
        {
            int damage = Mathf.CeilToInt(Mathf.Lerp(maxDamage, minDamage, distance));
            return damage;
        }

        // Calculate explosion force based on distance
        private int CalculateExplosionForce(float distance)
        {
            float t = Mathf.Clamp01(distance / explosionRadius);
            float explosionForce = Mathf.Lerp(minExplosionForce, maxExplosionForce, t);
            return Mathf.RoundToInt(explosionForce);
        }


        [PunRPC]
        public void triggerEffectRPC(Vector3 pos)
        {
            GameObject effect = Instantiate(explosionEffect, pos, Quaternion.identity);
            Destroy(effect, 2f);
        }

    }
}
