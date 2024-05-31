using DefaultNamespace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList instance;
    public GameObject roomManagerGameObject;
    public RoomManager roomManager;
    
    [Header("UI")]
    public Transform roomListParent;
    public GameObject roomListingPrefab;

    private List<RoomInfo> cashedRoomList = new List<RoomInfo>();

    public void ChangeRoomToCreatName(string roomName)
    {
        roomManager.roomNameToJoin = roomName;
    }
    
    
    void Awake()
    {
        instance = this;
    }
    
    private IEnumerator Start()
    {
        // precautions
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (cashedRoomList.Count <= 0)
        {
            cashedRoomList = roomList;
        }
        else
        {
            foreach (var room in roomList)
            {
                for (int i = 0; i < cashedRoomList.Count; i++)
                {
                    if (cashedRoomList[i].Name == room.Name)
                    {
                        List<RoomInfo> newList = cashedRoomList;

                        if (room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);

                        }
                        else
                        {
                            newList[i] = room;
                        }
                        cashedRoomList = newList;
                    }
                }
            }
        }

        UpdateUI();
    }
    private void UpdateUI()
    {
        foreach (Transform roomitem in roomListParent)
        {
            Destroy(roomitem.gameObject);
        }

        foreach (var room in cashedRoomList)
        {
            GameObject roomItem = Instantiate(roomListingPrefab, roomListParent);

            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + " / " + room.MaxPlayers;
            
            roomItem.GetComponent<RoomItemButton>().roomName = room.Name;
        }
    }
    
    public void JoinRoomByName(string roomName)
    {
        roomManager.roomNameToJoin = roomName;
        roomManagerGameObject.SetActive(true);
        
    }

}
