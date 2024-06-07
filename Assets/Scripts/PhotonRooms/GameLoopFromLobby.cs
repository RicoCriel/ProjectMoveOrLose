using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
namespace DefaultNamespace.PhotonRooms
{
    public class GameLoopFromLobby : MonoBehaviourPunCallbacks
    {
        [FormerlySerializedAs("GameLoopUI")]
        [Header("GameLoopUI")]
        [SerializeField]
        private Transform GameLoopTimerUI;

        [SerializeField]
        private TextMeshProUGUI FightText;

        [SerializeField]
        private TextMeshProUGUI Timer;
        [SerializeField]
        private Image TimerImage;

        [FormerlySerializedAs("_repalyScreen")]
        [Header("ReplayScreen")]
        [SerializeField]
        private GameObject _replayScreen;

        [Header("PauseScreen")]
        [SerializeField]
        private GameObject _pauseScreen;

        [Header("GameLoopSettings")]
        public GameObject playerPrefabs;
        public Transform[] spawnPoints;

        //local player
        private PlayerMovement player;
        private PhotonView playerPV;
        private PhotonView view;

        [SerializeField] private string _sceneToReload;
        [SerializeField] private string _SceneReturnToLobby;

        private NotificationManager notificationManager;

        private bool allPlayersReady = false;

        private int totalPlayers;
        private int readyPlayers;


        private void Awake()
        {
            notificationManager = FindObjectOfType<NotificationManager>();
            view = GetComponent<PhotonView>();
            _replayScreen.SetActive(false);
            _pauseScreen.SetActive(false);
            GameLoopTimerUI.gameObject.SetActive(true);


            FightText.transform.localScale = Vector3.zero;
            Sequence fightSequence = DOTween.Sequence();
            fightSequence.Append(FightText.transform.DOScale(1.2f, 0.5f));
            fightSequence.Append(FightText.transform.DOScale(1f, 0.5f));
            fightSequence.Play();
            FightText.gameObject.SetActive(true);


            FightText.text = "Waiting for all players to Connect";
            Timer.text = "";
            TimerImage.fillAmount = 0;

            view.RPC("PlayerJoinedGameMessage", RpcTarget.All);
        }

        private void Start()
        {
            Debug.Log("Game scene started");
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("TotalPlayers", out object property))
            {
                totalPlayers = (int)property;
                Debug.Log("Total Players: " + totalPlayers);
            }

            // Set player ready property
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerReady", true }
            });
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerDead", false }
            });

            Debug.Log("Player marked as ready");
            PlayerSpawn();
            // view.RPC("AllPlayersReadyRPC", RpcTarget.All);
            CheckAllPlayersReady();
        }

        
        private void CheckAllPlayersReady()
        {
            // Only the master client should check player readiness
            if (!PhotonNetwork.IsMasterClient) return;

            StartCoroutine(CheckAllPlayersReadyCoroutine());
        }

        private IEnumerator CheckAllPlayersReadyCoroutine()
        {
            while (true)
            {
                readyPlayers = 0;
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (player.CustomProperties.ContainsKey("PlayerReady") && (bool)player.CustomProperties["PlayerReady"])
                    {
                        readyPlayers++;
                    }
                }

                if (readyPlayers == totalPlayers)
                {
                    // All players are ready, start the game

                    view.RPC("AllPlayersReadyRPC", RpcTarget.All);
                    allPlayersReady = true;
                    yield break;
                }

                yield return new WaitForSeconds(1f);
            }
        }

        [PunRPC]
        private void AllPlayersReadyRPC()
        {
            allPlayersReady = true;

            StartCoroutine(CountdownCoroutine());
        }



        private IEnumerator CountdownCoroutine()
        {
            yield return new WaitForSeconds(1f);

            GameLoopTimerUI.gameObject.SetActive(true);
            FightText.gameObject.SetActive(true);

            FightText.text = "GET READY";
            FightText.transform.localScale = Vector3.zero;
            Sequence fightSequence2 = DOTween.Sequence();
            fightSequence2.Append(FightText.transform.DOScale(1.2f, 0.5f));
            fightSequence2.Append(FightText.transform.DOScale(1f, 0.5f));
            fightSequence2.Play();

            for (int i = 5; i >= 0; i--)
            {
                Timer.text = i.ToString();
                TimerImage.fillAmount = 0;
                TimerImage.DOFillAmount(1, 1f);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(Timer.transform.DOScale(1.2f, 0.5f));
                sequence.Append(Timer.transform.DOScale(1f, 0.5f));
                sequence.Play();

                yield return new WaitForSeconds(1f);
            }

            Timer.gameObject.SetActive(false);
            FightText.gameObject.SetActive(true);
            FightText.text = "Fight";
            FightText.transform.localScale = Vector3.zero;
            Sequence fightSequence = DOTween.Sequence();
            fightSequence.Append(FightText.transform.DOScale(1.2f, 0.5f));
            fightSequence.Append(FightText.transform.DOScale(1f, 0.5f));
            fightSequence.Play();

            player.EnablePlayer();

            yield return new WaitForSeconds(1f);
            GameLoopTimerUI.gameObject.SetActive(false);
        }

        public void PlayerSpawn()
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];
            Player LocalPlayer = PhotonNetwork.LocalPlayer;

            // Color playerColor = (color)PhotonNetwork.LocalPlayer.CustomProperties["playerColor"];

            GameObject InstanciatedPlayer = PhotonNetwork.Instantiate(playerPrefabs.name, spawnPoint.position, spawnPoint.rotation);
            player = InstanciatedPlayer.transform.GetComponent<PlayerMovement>();
            playerPV = player.GetComponent<PhotonView>();
            player.InitializePlayer(LocalPlayer, Color.white);
            player.DisablePlayer();

            // view.RPC("SpawnPlayerRPC", RpcTarget.All);
        }


        // [PunRPC]
        // private void SpawnPlayerRPC()
        // {
        //     int spawnIndex = Random.Range(0, spawnPoints.Length);
        //     Transform spawnPoint = spawnPoints[spawnIndex];
        //     Player LocalPlayer = PhotonNetwork.LocalPlayer;
        //     // int playerAvatar = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"];
        //     // Color playerColor = (color)PhotonNetwork.LocalPlayer.CustomProperties["playerColor"];
        //
        //     PhotonNetwork.Instantiate(playerPrefabs.name, spawnPoint.position, spawnPoint.rotation);
        //     player = playerPrefabs.GetComponent<PlayerMovement>();
        //     player.InitializePlayer(LocalPlayer, Color.white);
        //     player.DisablePlayer();
        //
        //
        // }

        private void Update()
        {
            if (!allPlayersReady)
            {
                return;
            }

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

                HandleLocalPauseScreen();

            }
        }
        public void HandleLocalPauseScreen()
        {
            if (playerPV.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (!_pauseScreen.activeSelf) // If the pause screen is not active
                    {
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        player.CanMove = false;
                        _pauseScreen.SetActive(true); // Activate the pause screen
                    }
                    else // If the pause screen is active
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        player.CanMove = true;
                        _pauseScreen.SetActive(false); // Deactivate the pause screen
                    }
                }
            }
        }

        private void HandlePlayerDeath()
        {
            PhotonNetwork.Destroy(player.gameObject);

            view.RPC("Endgame", RpcTarget.All);
      
        }

        [PunRPC]
        void Endgame(string id)
        {
            _replayScreen.SetActive(true);
            player.CanMove = false;
            // PhotonNetwork.Destroy(player.gameObject);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public void ReloadScene()
        {
            // view.RPC("ReloadSceneRPC", RpcTarget.All);
            PhotonNetwork.LoadLevel(_sceneToReload);
        }
        [PunRPC]
        public void ReloadSceneRPC()
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            PhotonNetwork.LoadLevel(_sceneToReload);
        }

        public void BackToLobby()
        {
            view.RPC("PlayerLeftGameMessage", RpcTarget.All);
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "PlayerReady", false }
            });
            
         
            
            PhotonNetwork.Destroy(player.gameObject);
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(_SceneReturnToLobby);
        }

        [PunRPC]
        public void PlayerLeftGameMessage()
        {
            notificationManager.ShowNotification("Player " + PhotonNetwork.LocalPlayer.NickName + " has left the game");
           
        }

        [PunRPC]
        public void PlayerJoinedGameMessage()
        {
            notificationManager.ShowNotification("Player " + PhotonNetwork.LocalPlayer.NickName + " has joined the game");
        }


        [PunRPC]
        public void BackToLobbyRPC()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel(_SceneReturnToLobby);
        }



    }
}
