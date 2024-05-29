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
        [SerializeField] GameObject sparkEffect;

        bool exploded = false;
        bool collHappened = false;
        Vector3 collisionpoint;

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<PhotonView>();
            }

            StartCoroutine(KillMe(1.5f));
        }

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

            if (PhotonNetwork.IsMasterClient)
            {
                Explosion();
            }
        }

        public void Explosion()
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
            }

            view.RPC("triggerEffectRPC", RpcTarget.All, transform.position);

            Collider[] Playercolliders = Physics.OverlapSphere(explosionPoint, explosionRadius * radiusDestroyMultiplier);

            foreach (var hit in Playercolliders)
            {
                if (hit.tag == "Player")
                {
                    PhotonView component = hit.GetComponent<PhotonView>();
                    BombManager.instance.PushTarget(component.ViewID, explosionForce, hit.transform.position, explosionRadius);
                }
            }

            Collider[] BlockCollider = Physics.OverlapSphere(explosionPoint, explosionRadius);
            foreach (var hit in BlockCollider)
            {
                if (hit.tag == "Block")
                {
                    //float distance = Vector3.Distance(transform.position, hit.transform.position);
                    //int calculatedDamage = CalculateDamage(distance);

                    //MapGenerator.instance.DamageBlock(hit.transform.position, calculatedDamage);
                }
            }
            MapGenerator.instance.SetRoomDirty();
            BombManager.instance.DestroyBomb(view.ViewID);
        }

        private int CalculateDamage(float distance)
        {
            int damage = Mathf.CeilToInt(Mathf.Lerp(maxDamage, minDamage, distance));
            return damage;
        }

        private int CalculateExplosionForce(float distance)
        {
            float t = Mathf.Clamp01(distance / explosionRadius);
            float explosionForce = Mathf.Lerp(minExplosionForce, maxExplosionForce, t);
            return Mathf.RoundToInt(explosionForce);
        }

        private IEnumerator KillMe(float cd)
        {
            yield return new WaitForSeconds(cd);
            PhotonNetwork.Destroy(gameObject);
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
}


