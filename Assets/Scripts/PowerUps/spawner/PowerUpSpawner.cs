using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace DefaultNamespace.PowerUps.spawner
{
    public class PowerUpSpawner : MonoBehaviourPunCallbacks
    {
        [Header("SpawnTimer")]
        [SerializeField] private float _timeBetweenSpawns = 10f;
       private float _currentTimer = 0f;


        [SerializeField] private int AmountToSpawn = 5;
        private int _amountSpawned = 0;

        private Dictionary<PowerUpType, PowerUpBase> _powerUpsToSpawn;



        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            _currentTimer += Time.deltaTime;
            VisualizeNextSpawnTimer();
            if (_currentTimer >= _timeBetweenSpawns)
            {
                _amountSpawned++;
                _currentTimer = 0;
                SpawnPowerUp();
                VisualiseNextSpawn();
            }
        }

        public void SetPowerUpsToSpawn(Dictionary<PowerUpType, PowerUpBase> powerUpsToSpawn)
        {
            _powerUpsToSpawn = new Dictionary<PowerUpType, PowerUpBase>();
            foreach (KeyValuePair<PowerUpType, PowerUpBase> powerUp in powerUpsToSpawn)
            {
                _powerUpsToSpawn.Add(powerUp.Key, powerUp.Value);
            }
        }


        private void VisualiseNextSpawn()
        {
            if (_amountSpawned >= AmountToSpawn)
            {
                //todo send event this spawner is done
            }
        }

        private void SpawnPowerUp()
        {
            PowerUpBase toStpawn = GetRandomKeyValuePair(_powerUpsToSpawn).Value;

            GameObject spawnedPopup = PhotonNetwork.Instantiate(toStpawn.name, transform.position, Quaternion.identity);
        }

        private void VisualizeNextSpawnTimer()
        {
            float TimerPercentage = _currentTimer / _timeBetweenSpawns;
            
            //todo implement some shader/ visualizsation that counts down/ up
        }


        private KeyValuePair<PowerUpType, PowerUpBase> GetRandomKeyValuePair(Dictionary<PowerUpType, PowerUpBase> dictionary)
        {
            // Convert the dictionary to an array of KeyValuePair objects
            KeyValuePair<PowerUpType, PowerUpBase>[] kvpArray = new KeyValuePair<PowerUpType, PowerUpBase>[dictionary.Count];
            int index = 0;
            foreach (KeyValuePair<PowerUpType, PowerUpBase> kvp in dictionary)
            {
                kvpArray[index] = kvp;
                index++;
            }

            // Get a random index
            int randomIndex = RandomSystem.GetRandomInt(0, dictionary.Count);

            // Return the random key-value pair
            return kvpArray[randomIndex];
        }
    }
}
