using Photon.Pun;
using UnityEngine;
namespace DefaultNamespace
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager instance;
        
        public string GameSceneToLoadString = "Game";
        public string roomNameToJoin;

        private void Awake()
        {
            instance = this;
        }
        
        public void JoinRoomButtonPressed()
        {
            RandomSystem.SetSeed(roomNameToJoin);
            
            PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
            
            // PhotonNetwork.JoinRoom(roomName);
        }
        
        // public override void OnConnectedToMaster()
        // {
        //     base.OnConnectedToMaster();
        //     
        //     Debug.Log("Connected To Server");
        //     
        //     PhotonNetwork.JoinLobby();
        // }
        //
        // public override void OnJoinedLobby()
        // {
        //     base.OnJoinedLobby();
        //     
        //     PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
        //     
        //     Debug.Log("connected");
        //     
        //     //spawnPlayers
        // }
        
        // public override void OnJoinedRoom()
        //
        // {
        //     Debug.Log("Successfully joined room: " + PhotonNetwork.CurrentRoom.Name);
        //     PhotonNetwork.LoadLevel(GameSceneToLoadString);
        // }
    }
}
