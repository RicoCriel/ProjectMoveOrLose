using System.Collections.Generic;
using UnityEngine;
namespace DefaultNamespace
{
    public class BlockWithHealth : MonoBehaviour
    {
        [SerializeField]
        private int _health = 0;

        [SerializeField]
        private List<BlockData> blockData;

        [SerializeField]
        private MeshRenderer _myMeshRenderer;
        private MeshFilter _myMeshFilter;

        public void InitializeBlockWithHealth(int startingHealth)
        {
            _health = startingHealth;
            ChangeBlockView(_health); 
        }

        public bool TakeDamageAndCheckIfDead(int damage)
        {
            _health -= damage;
            ChangeBlockView(_health);

            return _health <= 0;
        }
        private void ChangeBlockView(int CurrentHealth)
        {
            if (CurrentHealth < blockData.Count)
            {
                Debug.LogError("Health cannot be greater than the number of blocksData's");
            }
            else
            {
                _myMeshFilter.mesh = blockData[CurrentHealth].Mesh;
                _myMeshRenderer.material = blockData[CurrentHealth].Material;
            }
        }


    }

    [System.Serializable]
    public class BlockData
    {
        public Mesh Mesh;
        public Material Material;
    }
}
