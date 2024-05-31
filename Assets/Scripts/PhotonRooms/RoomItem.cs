using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace DefaultNamespace.PhotonRooms
{
    public class RoomItem : MonoBehaviour
    {
        public TextMeshProUGUI roomName;
        
        public TextMeshProUGUI playerCount;
        
       private  LobbyManager Manager;

        private void Start()
        {
            Manager = FindObjectOfType<LobbyManager>();
        }

        public void SetRoomName (string name)
        {
            roomName.text = name;
        }
        
        public void SetPlayerCount (int currentPlayers, int maxPlayers)
        {
            playerCount.text = currentPlayers + "/" + maxPlayers;
        }
        
        public void OnClickItem()
        {
            Manager.JoinRoom(roomName.text);
        }
    }
}
