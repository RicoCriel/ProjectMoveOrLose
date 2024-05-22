using Photon.Pun;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
namespace DefaultNamespace.PowerUps.spawner
{
    public class PowerUpSpawningManager : MonoBehaviourPunCallbacks
    {
        [FormerlySerializedAs("CenterMapSpawner")]
        [Header("CenterSpawner")]
        [SerializeField] private PowerUpSpawner CenterMapSpawnerPrefab;

        [FormerlySerializedAs("RandomSpawner")]
        [SerializeField] private PowerUpSpawner RandomSpawnerPrefab;
        [SerializeField] private int AmountOfNormalSpawners;

        [Header("Positioning")]
        [SerializeField] private List<Transform> SpawnerPositions = new List<Transform>();

        private List<Transform> availableSpawners = new List<Transform>();
        private Dictionary<Transform, PowerUpSpawner> currentActiveSpawners = new Dictionary<Transform, PowerUpSpawner>();

        private PhotonView _photonView;

        public void Start()
        {
            _photonView = GetComponent<PhotonView>();

            if (PhotonNetwork.IsMasterClient)
            {
                GameObject CenterSpawner = PhotonNetwork.Instantiate(CenterMapSpawnerPrefab.name, MapGenerator.instance.MapCenter, quaternion.identity);
                CenterSpawner.transform.parent = transform;
                PowerUpSpawner centerSpawnerPUS = CenterSpawner.GetComponent<PowerUpSpawner>();
                centerSpawnerPUS.MinAmountToSpawn = int.MaxValue;
                centerSpawnerPUS.MaxAmountToSpawn = int.MaxValue;

                // centerSpawnerPUS.SpawningDone += (s, e) =>
                // {
                //     todo hook up events if we want the gravety spawner to run out of spawns ( we dont kekw)
                //     ReplaceEmptySpawner(CenterSpawner.transform);
                // };

                availableSpawners.AddRange(SpawnerPositions);

                if (CheckIfenoughSpawnPositions()) return;

                SpawnStartingSpawners();
            }
        }
        private bool CheckIfenoughSpawnPositions()
        {
            if (availableSpawners.Count < AmountOfNormalSpawners)
            {
                Debug.Log("Not enough spawners to spawn all powerups");
                return true;
            }
            return false;
        }

        private void SpawnStartingSpawners()
        {
            for (int i = 0; i <= AmountOfNormalSpawners; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableSpawners.Count);
                // if (!currentActiveSpawners.ContainsKey(availableSpawners[randomIndex]))
                // {
                    Vector3 spawnPos = availableSpawners[randomIndex].position;
                    
                    availableSpawners.RemoveAt(randomIndex);
                    GameObject spawnerGo = PhotonNetwork.Instantiate(RandomSpawnerPrefab.name, spawnPos, quaternion.identity);
                    spawnerGo.transform.parent = transform;
                    PowerUpSpawner spawner = spawnerGo.GetComponent<PowerUpSpawner>();
                    currentActiveSpawners.Add(availableSpawners[randomIndex], spawner);

                    spawner.SpawningDone += (s, e) => {
                        ReplaceEmptySpawner(availableSpawners[randomIndex]);
                    };
                // }
            }
        }

        private void ReplaceEmptySpawner(Transform freedSpawner)
        {
            if (currentActiveSpawners.ContainsKey(freedSpawner))
            {
                currentActiveSpawners.Remove(freedSpawner);
                availableSpawners.Add(freedSpawner);
            }

            int randomIndex = UnityEngine.Random.Range(0, availableSpawners.Count);
            // if (!currentActiveSpawners.ContainsKey(availableSpawners[randomIndex]))
            // {
                Vector3 spawnPos = availableSpawners[randomIndex].position;
                
                availableSpawners.RemoveAt(randomIndex);
                GameObject spawnerGo = PhotonNetwork.Instantiate(RandomSpawnerPrefab.name, spawnPos, quaternion.identity);
                spawnerGo.transform.parent = transform;
                PowerUpSpawner spawner = spawnerGo.GetComponent<PowerUpSpawner>();
                currentActiveSpawners.Add(availableSpawners[randomIndex], spawner);

                spawner.SpawningDone += (s, e) => {
                    ReplaceEmptySpawner(availableSpawners[randomIndex]);
                };
            // }
        }
    }
}
