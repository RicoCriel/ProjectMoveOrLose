using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsStats : MonoBehaviourPunCallbacks
{
    private float deltaTime = 0.0f;
    public TextMeshProUGUI statsText;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        if(PhotonNetwork.IsConnected)
        {
            int ping = PhotonNetwork.GetPing();
            statsText.text = string.Format("FPS: {0:0.} \nPing: {1} ms", fps, ping);
        }

    }
}
