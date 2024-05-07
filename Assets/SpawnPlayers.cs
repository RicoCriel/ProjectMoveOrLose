using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;


public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    [SerializeField]private GameObject Canvas;
    [SerializeField]private TMP_Text playerWinScreen;
    [SerializeField]private GameObject startbuttonobject;
    [SerializeField]private GameObject ReloadSceneButton;
    private GameObject player;

    [SerializeField] private string _sceneToReload;

    [SerializeField] private GameObject spawnlocation; 
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    private PhotonView view;

    private void Awake()
    {
        
        view = GetComponent<PhotonView>();
        startbuttonobject.SetActive(false);
        ReloadSceneButton.SetActive(false);
        Canvas.SetActive(true);
        view.RPC("StartButton",RpcTarget.MasterClient);
    }
    [PunRPC]

    void StartButton()
    {
        
        startbuttonobject.SetActive(true);

    }
    [PunRPC]

    void ReloadScene()
    {
        
        ReloadSceneButton.SetActive(true);

    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        view.RPC("ReloadSceneButRPC",RpcTarget.All);
    }
    public void ReloadSceneBut()
    {
        view.RPC("ReloadSceneButRPC",RpcTarget.All);
        // PhotonNetwork.LoadLevel(_sceneToReload);

    }
    [PunRPC]
    public void ReloadSceneButRPC()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        PhotonNetwork.LoadLevel(_sceneToReload);
        

    }
    

    public void PlayerSpawn()
    {
        view.RPC("PlayerSpawnRPC",RpcTarget.All);
        
    }

    [PunRPC]

    void PlayerSpawnRPC()
    {
        Vector3 randomPosition = new Vector3(spawnlocation.transform.position.x +UnityEngine.Random.Range(minX, maxX),
            spawnlocation.transform.position.y,
            spawnlocation.transform.position.z +UnityEngine.Random.Range(minY, maxY));
        player = PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
        startbuttonobject.SetActive(false);
        Canvas.SetActive(false);
       MapGenerator.instance.startAutoDestroyBlocks();
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.transform.position.y < -7.5f)
                        {
                            view.RPC("ReloadScene",RpcTarget.MasterClient);
                            view.RPC("Endgame",RpcTarget.All, PhotonNetwork.LocalPlayer.UserId);
                        }
        }
            
        
    }

  

    [PunRPC]
    void Endgame(string id)
    {
        playerWinScreen.text = "Player " + id + " Wins";
        Canvas.SetActive(true);
        PhotonNetwork.Destroy(player);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
}