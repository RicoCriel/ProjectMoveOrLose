using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using TMPro;


public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    
    public TMP_InputField CreateInput;
    public TMP_InputField JoinInput;
    // Start is called before the first frame update

    private void Awake()
    {
        CreateInput.text = "DefaultRoom";
        JoinInput.text = "DefaultRoom";
        
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(CreateInput.text);
    }
    
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(JoinInput.text);
    }
    
    public override void OnJoinedRoom()
    
    {
        Debug.Log("Successfully joined room: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("Game");
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room: " + message);
    }
   
}
