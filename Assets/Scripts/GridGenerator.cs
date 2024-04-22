using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int _gridSizeX = 10;
    [SerializeField] private int _gridSizeZ = 10;
    [SerializeField] private float _minGridHeight = 1f;
    [SerializeField] private float _maxGridHeight = 5f;
    [SerializeField] private GameObject[] _tilePrefabs; // Array of tile prefabs
    [SerializeField] private float _tileHeight = 1f; // Height of each tile prefab

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
                float gridHeight = Random.Range(_minGridHeight, _maxGridHeight);
                float startY = gridHeight + (_tileHeight * 2); // Start height for the top tile

                for (int i = 0; i < 3; i++) // Instantiate 3 different tile prefabs under each other
                {
                    GameObject selectedPrefab = _tilePrefabs[i];
                    Vector3 position = new Vector3(x, startY - (i * _tileHeight), z);
                    GameObject newTile = Instantiate(selectedPrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
