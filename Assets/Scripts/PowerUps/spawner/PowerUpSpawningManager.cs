using Photon.Pun;
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

        [Header("Other Spawners")]
        [SerializeField] private List<Transform> SpawnerPositions = new List<Transform>();
        private List<Transform> availableSpawners = new List<Transform>();
        private Dictionary<Transform, PowerUpSpawner> currentActiveSpawners = new Dictionary<Transform, PowerUpSpawner>();

        [SerializeField] private int AmountOfNormalSpawners;
        [FormerlySerializedAs("RandomSpawner")]
        [SerializeField] private PowerUpSpawner RandomSpawnerPrefab;

        [Header("PowerUps")]
        Dictionary<PowerUpType, PowerUpBase> AllPowerUps = new Dictionary<PowerUpType, PowerUpBase>();



        public void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameObject CenterSpawner = PhotonNetwork.Instantiate(CenterMapSpawnerPrefab.name, MapGenerator.instance.MapCenter, quaternion.identity);

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
            for (int i = 0; i < AmountOfNormalSpawners; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableSpawners.Count);
                if (!currentActiveSpawners.ContainsKey(availableSpawners[randomIndex]))
                {
                    availableSpawners.RemoveAt(randomIndex);
                    GameObject spawnerGo = PhotonNetwork.Instantiate(RandomSpawnerPrefab.name, MapGenerator.instance.GetFreePosition(), quaternion.identity);
                    PowerUpSpawner spawner = spawnerGo.GetComponent<PowerUpSpawner>();
                    currentActiveSpawners.Add(availableSpawners[randomIndex], spawner);

                    //todo hook up event if the spawner is out of stuff to spawn/ amount of spawns
                }
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
            if (!currentActiveSpawners.ContainsKey(availableSpawners[randomIndex]))
            {
                availableSpawners.RemoveAt(randomIndex);
                GameObject spawnerGo = PhotonNetwork.Instantiate(RandomSpawnerPrefab.name, MapGenerator.instance.GetFreePosition(), quaternion.identity);
                PowerUpSpawner spawner = spawnerGo.GetComponent<PowerUpSpawner>();
                currentActiveSpawners.Add(availableSpawners[randomIndex], spawner);

                //todo hook up event if the spawner is out of stuff to spawn/ amount of spawns
            }
        }
    }
}
