using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using Photon.Realtime;
using TMPro;


public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    
    public TMP_InputField CreateInput;
    public TMP_InputField JoinInput;
    
    public string GameSceneToLoadString = "Game";
    // Start is called before the first frame update

    private void Awake()
    {
        CreateInput.text = "DefaultRoom";
        JoinInput.text = "DefaultRoom";
        
    }

    public void CreateRoom()
    {
        RandomSystem.SetSeed(CreateInput.text);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(CreateInput.text,roomOptions);
    }
    
    public void JoinRoom()
    {
        RandomSystem.SetSeed(JoinInput.text);
        PhotonNetwork.JoinRoom(JoinInput.text);
    }
    
    // public override void OnJoinedRoom()
    //
    // {
    //     Debug.Log("Successfully joined room: " + PhotonNetwork.CurrentRoom.Name);
    //     PhotonNetwork.LoadLevel(GameSceneToLoadString);
    // }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room: " + message);
    }
   
}
