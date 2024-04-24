using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyMyself());
    }

    IEnumerator DestroyMyself()
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.Destroy(this.gameObject);
    }
    
}
