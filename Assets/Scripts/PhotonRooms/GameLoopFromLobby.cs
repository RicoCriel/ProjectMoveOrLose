using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
namespace DefaultNamespace.PhotonRooms
{
    public class GameLoopFromLobby : MonoBehaviourPun
    {
        [Header("GameLoopUI")]
        [SerializeField]
        private Transform GameLoopUI;
        
        [SerializeField]
        private TextMeshProUGUI FightText;
        
        [SerializeField]
        private TextMeshProUGUI Timer;
        [SerializeField]
        private Image TimerImage;

        [Header("GameLoopSettings")]
        public GameObject playerPrefabs;
        public Transform[] spawnPoints;

        //local player
        private PlayerMovement player;
        
        private PhotonView view;

        private void Awake()
        {
            view = GetComponent<PhotonView>();
        }

        private void Start()
        {
            SpawnPlayer();
            StartCoroutine(CountdownCoroutine());
        }
      

        private IEnumerator CountdownCoroutine()
        {
            FightText.gameObject.SetActive(false);
            for (int i = 5; i >= 0; i--)
            {
                Timer.text = i.ToString();
                TimerImage.fillAmount = i / 5f;

                Sequence sequence = DOTween.Sequence();
                sequence.Append(Timer.transform.DOScale(1.2f, 0.5f));
                sequence.Append(Timer.transform.DOScale(1f, 0.5f));
                sequence.Play();

                yield return new WaitForSeconds(1f);
            }

            Timer.gameObject.SetActive(false);
            FightText.gameObject.SetActive(true);
            FightText.text = "Fight";

            Sequence fightSequence = DOTween.Sequence();
            fightSequence.Append(FightText.transform.DOScale(1.2f, 0.5f));
            fightSequence.Append(FightText.transform.DOScale(1f, 0.5f));
            fightSequence.Play();
            
            player.EnablePlayer();
        }
        
        private void SpawnPlayer()
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[spawnIndex];
            Player LocalPlayer = PhotonNetwork.LocalPlayer;
            int playerAvatar = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"];
            // Color playerColor = (color)PhotonNetwork.LocalPlayer.CustomProperties["playerColor"];

            PhotonNetwork.Instantiate(playerPrefabs.name, spawnPoint.position, spawnPoint.rotation);
            player = playerPrefabs.GetComponent<PlayerMovement>();
            player.InitializePlayer(LocalPlayer, Color.white);
            // playerMovement.InitializePlayer(player);

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
            PhotonNetwork.Destroy(player.gameObject);
            
            
            // SpawnPlayer();
        }
    }
}
