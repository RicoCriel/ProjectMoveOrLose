using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace DefaultNamespace.PowerUps.spawner
{
    public class PowerUpSpawner : MonoBehaviourPunCallbacks
    {
        [Header("SpawnTimer")]
        [SerializeField] private float _timeBetweenSpawns = 10f;
        private float _currentTimer = 0f;

        [SerializeField] public int AmountToSpawn = 5;
        private int _amountSpawned = 0;

        [Header("PowerUps")]
        [SerializeField] private List<PowerUpBase> _powerUpsToSpawn;

        Dictionary<PowerUpType, PowerUpBase> AllPowerUps = new Dictionary<PowerUpType, PowerUpBase>();

        PowerUpBase _currentPowerUp;
        private PhotonView _PhotonView;

        private void Awake()
        {
            _PhotonView = GetComponent<PhotonView>();

            if (!PhotonNetwork.IsMasterClient)
                return;

            foreach (PowerUpBase powerUp in _powerUpsToSpawn)
            {
                AllPowerUps.TryAdd(powerUp._myPowerUpType, powerUp);
            }
        }

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

        private void VisualiseNextSpawn()
        {
            if (_amountSpawned >= AmountToSpawn)
            {
                TryDestroyActivespawnedPowerUp();
                OnSpawningDone(new SpawnerDoneEventArgs());
                PhotonNetwork.Destroy(gameObject);
            }
        }

        private void SpawnPowerUp()
        {
            TryDestroyActivespawnedPowerUp();

            PowerUpBase toSpawn = GetRandomKeyValuePair(AllPowerUps).Value;

            GameObject spawnedPopup = PhotonNetwork.Instantiate(toSpawn.name, transform.position, Quaternion.identity);
            spawnedPopup.transform.parent = transform;
            _currentPowerUp = spawnedPopup.GetComponent<PowerUpBase>();
        }
        private void TryDestroyActivespawnedPowerUp()
        {
            if (_currentPowerUp != null)
            {
                PhotonNetwork.Destroy(_currentPowerUp.gameObject);
            }
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

        public event EventHandler<SpawnerDoneEventArgs> SpawningDone;

        public virtual void OnSpawningDone(SpawnerDoneEventArgs eventargs)
        {
            EventHandler<SpawnerDoneEventArgs> handler = SpawningDone;
            handler?.Invoke(this, eventargs);
        }
    }

    public class SpawnerDoneEventArgs : EventArgs
    {
        public SpawnerDoneEventArgs()
        {

        }
    }
}
