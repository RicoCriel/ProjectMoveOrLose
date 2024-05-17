using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;


public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    [SerializeField] private GameObject Canvas;
    [SerializeField] private TMP_Text playerWinScreen;
    [SerializeField] private GameObject startbuttonobject;
    [SerializeField] private GameObject ReloadSceneButton;
    private GameObject player;

    [SerializeField] private string _sceneToReload;



    private PhotonView view;

    private void Awake()
    {

        view = GetComponent<PhotonView>();
        startbuttonobject.SetActive(false);
        ReloadSceneButton.SetActive(false);
        Canvas.SetActive(true);
        view.RPC("StartButton", RpcTarget.MasterClient);
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
        view.RPC("ReloadSceneButRPC", RpcTarget.All);
    }
    public void ReloadSceneBut()
    {
        view.RPC("ReloadSceneButRPC", RpcTarget.All);
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
        view.RPC("PlayerSpawnRPC", RpcTarget.All);

    }

    [PunRPC]
    void PlayerSpawnRPC()
    {
        Vector3 mapCenter = MapGenerator.instance.MapCenter;
        int randomDirection = UnityEngine.Random.Range(0, MapGenerator.instance.CubeDirections.Length);

        RaycastHit hit;
        if (Physics.Raycast(mapCenter, MapGenerator.instance.CubeDirections[randomDirection], out hit, 100))
        {
            player = PhotonNetwork.Instantiate(playerPrefab.name, hit.point, Quaternion.identity);
        }
        else
        {
            player = PhotonNetwork.Instantiate(playerPrefab.name, mapCenter, Quaternion.identity);
        }

        QuakeCharController charcontroller = player.gameObject.GetComponent<QuakeCharController>();
        // charcontroller.SetGravity(-directions[randomDirection] * 10);

        startbuttonobject.SetActive(false);
        Canvas.SetActive(false);

    }

    private void Update()
    {
        if (player != null)
        {
            if (player.transform.position.y < -7.5f)
            {
                view.RPC("ReloadScene", RpcTarget.MasterClient);
                view.RPC("Endgame", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId);
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
