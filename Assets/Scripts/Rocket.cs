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
            BombManager.instance.DestroyBomb(view.ViewID);
            
            Collider[] Playercolliders = Physics.OverlapSphere(explosionPoint, explosionRadius);
            
            foreach (var hit in Playercolliders)
            {
                
                if (hit.tag == "Player")
                {
                    Debug.Log("Pushing Player with id" + hit.GetComponent<PhotonView>().ViewID);
                    BombManager.instance.PushTarget(hit.GetComponent<PhotonView>().ViewID, explosionForce, explosionPoint, explosionRadius);
                }

                
                
            }
            
            Collider[] BlockCollider = Physics.OverlapSphere(explosionPoint, explosionRadius /2);
            foreach (var hit in BlockCollider)
            {
                if (hit.tag == "Block")
                {
                    MapGenerator.instance.DestroyBlock(hit.transform.position);
                    MapGenerator.instance.SetRoomDirty();
                }

                
                
            }

        }

        [PunRPC]
        void triggerEffectRPC(Vector3 pos)
        {
            GameObject effect = Instantiate(explosionEffect, pos, Quaternion.identity);
            Destroy(effect, 2f);
        }

    }
}
