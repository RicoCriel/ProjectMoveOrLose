using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace DefaultNamespace
{
    public class BlockWithHealth : MonoBehaviour
    {
        private int _health = 0;
        
        private int _maxHealth = 0;

        [SerializeField]
        private List<BlockData> blockData;

        [SerializeField]
        private MeshRenderer _myMeshRenderer;

        [SerializeField]
        private MeshFilter _myMeshFilter;
        
        public BlockType blockType;
        private static readonly int Lerpamount = Shader.PropertyToID("_lerpamount");
        private static readonly int Offset = Shader.PropertyToID("_Offset");

        private void Awake()
        {
            _myMeshRenderer.material.SetVector(Offset, new Vector2(Random.Range(0f, 10f) , Random.Range(0f, 10f)));
        }

        public void InitializeBlockWithHealth(int startingHealth)
        {
            _health = startingHealth;
            _maxHealth = startingHealth;
            ChangeBlockViewShader(_health);
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
                ChangeBlockViewShader(_health);
                return false;
            }
        }

        public int GetCurrentHealth()
        {
            return _health;
        }

        private void ChangeBlockView(int CurrentHealth)
        {
            if (blockType == BlockType.Chunk)
            {
                int tooUse =Mathf.CeilToInt( CurrentHealth / 2f);
                // _myMeshFilter.mesh = blockData[CurrentHealth - 1].Mesh;
                 _myMeshRenderer.material = blockData[tooUse - 1].Material;
            }
            else
            {
                // _myMeshFilter.mesh = blockData[CurrentHealth - 1].Mesh;
                _myMeshRenderer.material = blockData[CurrentHealth - 1].Material;
                
            }
        
            
        }
        
        private void ChangeBlockViewShader(int CurrentHealth)
        {
           float destroyedAmount = 1 - (float)CurrentHealth / _maxHealth;
           _myMeshRenderer.material.SetFloat(Lerpamount,destroyedAmount);
        }




    }

    [System.Serializable]
    public class BlockData
    {
        public Mesh Mesh;
        public Material Material;
    }
    
    public enum BlockType
    {
        Normal,
        Chunk
    }
}
