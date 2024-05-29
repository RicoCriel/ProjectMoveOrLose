using UnityEngine;
using Photon.Pun;
using System.Collections;

public class DestroyObject : MonoBehaviourPun
{
    [SerializeField] private float killTime;
    private Coroutine destroy;
    private void Start()
    {
        if(destroy != null)
            StopCoroutine(destroy);
        
        destroy = StartCoroutine(KillMe(killTime));
    }

    private IEnumerator KillMe(float time)
    {
        yield return new WaitForSeconds(time);
        this.gameObject.SetActive(false);
        Destroy(this.gameObject, time);
        //PhotonNetwork.Destroy(this.gameObject);
    }
}
