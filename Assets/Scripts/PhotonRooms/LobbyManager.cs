using DefaultNamespace.PhotonRooms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
   public TMP_InputField roomInputField;
   public Slider playerAmountSlider;
   public TextMeshProUGUI playerAmountText;
   public GameObject lobbyPanel;
   public GameObject roomPanel;
   public TextMeshProUGUI roomName;
   
   public RoomItem roomItemPrefab;
   public Transform content;
   List<RoomItem> roomItemsList = new List<RoomItem>();
   
   public float timeBetweenRoomUpdates = 1.5f;
   private float nextUpdateTime;

   private void Start()
   {
      PhotonNetwork.JoinLobby();
      OnPlayerAmountValueChanged();
   }
   
   public void OnPlayerAmountValueChanged()
   {
      playerAmountText.text = playerAmountSlider.value.ToString();
   }

   public void OnClickCreate()
   {
      if (roomInputField.text.Length >= 1)
      {
         RoomOptions roomOptions = new RoomOptions();
         roomOptions.MaxPlayers = (int)playerAmountSlider.value;
         PhotonNetwork.CreateRoom(roomInputField.text, roomOptions);
      }
     
   }
   
   public  override void OnJoinedRoom()
   {
      lobbyPanel.SetActive(false);
      roomPanel.SetActive(true);
      roomName.text ="Room Name: " + PhotonNetwork.CurrentRoom.Name;
   }
   
   public override void OnRoomListUpdate(List<RoomInfo> roomList)
   {
      if (Time.time > nextUpdateTime)
      {
         UpdateRoomList(roomList);
         nextUpdateTime = Time.time + timeBetweenRoomUpdates;
      }
  
   }
   private void UpdateRoomList(List<RoomInfo> List)
   {
      foreach (RoomItem item in roomItemsList)
      {
         Destroy(item.gameObject);
      }
      
      roomItemsList.Clear();
      
      foreach (RoomInfo room in List)
      {
         RoomItem item = Instantiate(roomItemPrefab, content);
         item.SetRoomName(room.Name);
         item.SetPlayerCount(room.PlayerCount, room.MaxPlayers);
         roomItemsList.Add(item);
      }
   }

   public void JoinRoom(string roomNameText)
   {
      PhotonNetwork.JoinRoom(roomNameText);
   }
   
   public void OnClickLeaveRoom()
   {
      PhotonNetwork.LeaveRoom();
      
   }
   
   public override void OnLeftRoom()
   {
      lobbyPanel.SetActive(true);
      roomPanel.SetActive(false);
   }
   
   public override void OnConnectedToMaster()
   {
      PhotonNetwork.JoinLobby();
   }
}
