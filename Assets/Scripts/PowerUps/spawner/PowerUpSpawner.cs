using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace DefaultNamespace.PowerUps.spawner
{
    public class PowerUpSpawner : MonoBehaviourPunCallbacks
    {
        [Header("SpawnTimer")]
        [SerializeField] private float _minTimeBetweenSpawns = 5f;
        [SerializeField] private float _maxTimeBetweenSpawns = 10f;
        private float currentTimeBetweenSpawns = int .MaxValue;
        private float _currentTimer = 0f;

        [SerializeField] public int MinAmountToSpawn = 5;
        [SerializeField] public int MaxAmountToSpawn = 5;
        private int AmountToSpawn = 0;
        private int _amountSpawned = 0;

        [Header("PowerUps")]
        [SerializeField] private List<PowerUpBase> _powerUpsToSpawn;

        private Dictionary<PowerUpType, PowerUpBase> AllPowerUps = new Dictionary<PowerUpType, PowerUpBase>();

        private PowerUpBase _currentPowerUp;
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
            
            AmountToSpawn = RandomSystem.GetRandomInt(MinAmountToSpawn, MaxAmountToSpawn);
            currentTimeBetweenSpawns = RandomSystem.GetRandomFloat(_minTimeBetweenSpawns, _maxTimeBetweenSpawns);
            //spawn initial
            _amountSpawned++;
            SpawnPowerUp();
            
            
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            _currentTimer += Time.deltaTime;
          
            VisualizeNextSpawnTimer();
            
            if (_currentTimer >= currentTimeBetweenSpawns)
            {
                currentTimeBetweenSpawns = RandomSystem.GetRandomFloat(_minTimeBetweenSpawns, _maxTimeBetweenSpawns);
                _amountSpawned++;
                SpawnPowerUp();
                _currentTimer = 0;
                VisualiseNextSpawn();
            }
        }

        private void VisualiseNextSpawn()
        {
            if (_amountSpawned >= AmountToSpawn)
            {
                TryDestroyActivespawnedPowerUp();
                OnSpawningDone(new SpawnerDoneEventArgs(this.transform));
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
            float TimerPercentage = _currentTimer / currentTimeBetweenSpawns;

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
        public Transform FreedSpawner{ get; }
        public SpawnerDoneEventArgs(Transform freedSpawner)
        {
            FreedSpawner = freedSpawner;
        }
    }
}
