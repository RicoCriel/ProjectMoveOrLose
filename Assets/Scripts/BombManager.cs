using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace DefaultNamespace
{
    public class BombManager : MonoBehaviourPunCallbacks
    {
        private PhotonView view;
        public static BombManager instance;

        private void Awake()
        {
            instance = this;
            view = GetComponent<PhotonView>();
        }

        public void PushTarget(int viewID, float explosionforce, Vector3 explosionPosition, float explosionRadius)
        {
            view.RPC("PushTargetRPC", RpcTarget.All, viewID, explosionforce, explosionPosition, explosionRadius);
        }

        [PunRPC]
        void PushTargetRPC(int viewID, float explosionforce, Vector3 explosionPosition, float explosionRadius)
        {
            PhotonView targetView = PhotonView.Find(viewID);
            if (targetView == null) return;
            if (!targetView.IsMine) return;
           targetView.GetComponent<QuakeCharController>()
                .AddImpact(explosionPosition, explosionforce);


            /*Rigidbody targetRigidbody = targetView.GetComponent<Rigidbody>();
            if (targetRigidbody == null) return;

            Vector3 direction = targetView.transform.position - explosionPosition;
            float distance = direction.magnitude;
            float force = explosionforce * (1 - distance / explosionRadius);
            targetRigidbody.AddForce(direction.normalized * force, ForceMode.Impulse);*/
        }

        public void DestroyBomb(int viewID)
        {
            view.RPC("DestroyBombRPC", RpcTarget.All, viewID);
        }

        [PunRPC]
        void DestroyBombRPC(int viewID)
        {
            PhotonView bombview = PhotonView.Find(viewID);
            if (bombview == null) return;
            if (!bombview.IsMine) return;

            PhotonNetwork.Destroy(bombview.gameObject);
        }
    }
}
