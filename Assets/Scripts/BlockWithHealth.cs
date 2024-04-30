using System.Collections.Generic;
using UnityEngine;
namespace DefaultNamespace
{
    public class BlockWithHealth : MonoBehaviour
    {
        private int _health = 0;

        [SerializeField]
        private List<BlockData> blockData;

        [SerializeField]
        private MeshRenderer _myMeshRenderer;

        [SerializeField]
        private MeshFilter _myMeshFilter;

        public void InitializeBlockWithHealth(int startingHealth)
        {
            _health = startingHealth;
            ChangeBlockView(_health);
        }

        public bool TakeDamageAndCheckIfDead(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                return true;
            }
            else
            {
                ChangeBlockView(_health);
                return false;
            }
        }

        public int GetCurrentHealth()
        {
            return _health;
        }

        private void ChangeBlockView(int CurrentHealth)
        {
           
                // _myMeshFilter.mesh = blockData[CurrentHealth - 1].Mesh;
                _myMeshRenderer.material = blockData[CurrentHealth - 1].Material;
            
        }




    }

    [System.Serializable]
    public class BlockData
    {
        public Mesh Mesh;
        public Material Material;
    }
}
