using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int _gridSizeX = 10;
    [SerializeField] private int _gridSizeZ = 10;
    [SerializeField] private int _chunkSize = 5; // Size of each noise chunk
    [SerializeField] private float _scale = 0.1f;
    [SerializeField] private float _heightMultiplier = 2f;
    [SerializeField] private GameObject[] _tilePrefabs; 
    
    private float _tileWidth = 1f; 
    private float _tileHeight = 1f; 

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int z = 0; z < _gridSizeZ; z++)
            {
                // Calculate chunk indices
                int chunkX = x / _chunkSize;
                int chunkZ = z / _chunkSize;

                // Generate Perlin noise for each chunk
                float perlinValue = Mathf.PerlinNoise((chunkX + transform.position.x) * _scale, (chunkZ + transform.position.z) * _scale);

                // Calculate grid height based on the noise value
                float gridHeight = perlinValue * _heightMultiplier;

                // Calculate startY for the top tile
                float startY = gridHeight + (_tileHeight * 2);

                // Instantiate tiles within the chunk
                for (int i = 0; i < _tilePrefabs.Length; i++)
                {
                    GameObject selectedPrefab = _tilePrefabs[i];
                    Vector3 position = new Vector3(x * _tileWidth, startY - (i * _tileHeight), z * _tileWidth);
                    GameObject newTile = Instantiate(selectedPrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
