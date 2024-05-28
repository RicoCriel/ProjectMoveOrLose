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
        int maxAttempts = 100;
        float spawnRadius = 2.0f; 
        float spawnSpacing = 1.0f; 

        Vector3 spawnPosition = FindSpawnPosition(mapCenter, spawnRadius, spawnSpacing, maxAttempts);
        if (spawnPosition == Vector3.zero)
        {
            spawnPosition = mapCenter;
        }

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        startbuttonobject.SetActive(false);
        Canvas.SetActive(false);
    }

    Vector3 FindSpawnPosition(Vector3 center, float radius, float spacing, int maxAttempts)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPosition = center + (UnityEngine.Random.insideUnitSphere * radius);
            randomPosition.y = center.y; 

            if (!IsPositionOccupied(randomPosition, spacing))
            {
                return randomPosition;
            }
        }
        return Vector3.zero; 
    }

    bool IsPositionOccupied(Vector3 position, float spacing)
    {
        Collider[] colliders = Physics.OverlapSphere(position, spacing);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        if (player != null)
        {
            Vector3 playerPosition = player.transform.position;

            // Check if the player is outside the x bounds
            if (playerPosition.x < MapGenerator.instance.XBoundaryDeath.X || playerPosition.x > MapGenerator.instance.XBoundaryDeath.Y)
            {
                HandlePlayerDeath();
            }
            // Check if the player is outside the y bounds
            else if (playerPosition.y < MapGenerator.instance.YBoundaryDeath.X || playerPosition.y > MapGenerator.instance.YBoundaryDeath.Y)
            {
                HandlePlayerDeath();
            }
            // Check if the player is outside the z bounds
            else if (playerPosition.z < MapGenerator.instance.ZBoundaryDeath.X || playerPosition.z > MapGenerator.instance.ZBoundaryDeath.Y)
            {
                HandlePlayerDeath();
            }
        }
    }

    private void HandlePlayerDeath()
    {
        view.RPC("ReloadScene", RpcTarget.MasterClient);
        view.RPC("Endgame", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId);
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
