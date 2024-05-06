using DefaultNamespace;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = System.Numerics.Vector2;

public class MapGenerator : MonoBehaviourPunCallbacks
{
    public static MapGenerator instance; // Singleton instance of MapGenerator

    public GameObject unitBlock; // Prefab for the basic building block

    [Header("BlocksWithHealth")]
    public BlockWithHealth blockWithHealth;
    public int BlockHealth = 4;
    public bool usingBlockWithHealth = false;

    public Transform blockHolder; // Parent object to hold all the blocks


    private PhotonView view; // Reference to the PhotonView component

    private LayerMask blockLayer; // Layer mask for blocks

    private Vector3 mapSize; /*= new Vector3(29, 3, 29);*/ // Size of the map

    [Header("BaseMapGen")]
    public int MapSizeXZ = 30; // Size of the map
    public int MapSizeY = 50; // Size of the map

    public int WallThickness = 1; // Thickness of the walls
    public int GroundHeight = 1; // Height of the ground

    [Header("AutoDestroyFloor")]
    public bool AutoDestroyBlocks = true;
    [SerializeField] private int blocksPerTick = 10;
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickInterval = 1f;


    [Header("mapClumps")]
    public BlockWithHealth blockWithHealthClump;
    public bool GenerateClumps = false;
    [Space]
    public int minclumpSize = 2;
    public int maxclumpSize = 4;
    [Space]
    public int minClumpAmount = 3;
    public int maxClumpAmount = 9;

    public int ClumpsLowerThanWalls = 5;

    public int BlockHealthClump = 8;

    [HideInInspector] public Vector2Int xBoundary /*= new Vector2Int(1, 30)*/; // X boundaries of the map
    [HideInInspector] public Vector2Int yBoundary /*= new Vector2Int(1, 50)*/; // Y boundaries of the map
    [HideInInspector] public Vector2Int zBoundary /*= new Vector2Int(1, 30)*/; // Z boundaries of the map

    public int[,,] mapState; // 3D array to store the state of each block in the map

    private float updateTimer = 5f; // Timer for updating room properties

    [HideInInspector] public bool roomDirty = false; // Flag to indicate if room properties need updating


    Dictionary<Vector3Int, BlockWithHealth> blocks = new Dictionary<Vector3Int, BlockWithHealth>();
    Dictionary<Vector3Int, BlockWithHealth> blocksGroundFloor = new Dictionary<Vector3Int, BlockWithHealth>();

    private int highestHealthBlocks;

    private void Awake()
    {
        highestHealthBlocks = Mathf.Max(BlockHealth, BlockHealthClump);

        instance = this; // Set the instance to this object

        mapSize = new Vector3(MapSizeXZ - 1, MapSizeY, MapSizeXZ - 1); // Set the map size
        xBoundary = new Vector2Int(1, MapSizeXZ); // Set the x boundaries
        yBoundary = new Vector2Int(1, MapSizeY + 20); // Set the y boundaries
        zBoundary = new Vector2Int(1, MapSizeXZ); // Set the z boundaries
        //
        // xBoundary = new Vector2Int((-MapSizeXZ /2) - 1, (MapSizeXZ/2) + 1); // Set the x boundaries
        // yBoundary = new Vector2Int((-MapSizeY /2 -1), (MapSizeY/2) + 1); // Set the y boundaries
        // zBoundary = new Vector2Int((-MapSizeXZ /2) -1 , (MapSizeXZ/2) +1); // Set the z boundaries

        view = GetComponent<PhotonView>(); // Get the PhotonView component attached to this object
        blockLayer = LayerMask.GetMask("Block"); // Get the layer mask for blocks
        blockLayer = ~blockLayer; // Invert the layer mask
    }

    private void Start()
    {
        mapState = new int[xBoundary.y, yBoundary.y, zBoundary.y]; // Initialize the map state array

        if (PhotonNetwork.IsMasterClient)
        {
            GenerateGrid(); // Generate a fresh map
            ZeroMap(); // Set the rest of the map to zero
            SetPunRoomProperties(); // Set Photon room properties
        }
        else
        {
            Debug.Log("Not master client, getting room data...");

            // Retrieve room data from Photon custom properties
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

            // Extract max boundaries from room properties
            Vector3Int maxBoundaries = new Vector3Int(
                Convert.ToInt32(properties["xMax"]),
                Convert.ToInt32(properties["yMax"]),
                Convert.ToInt32(properties["zMax"]));

            // Iterate over each slice of the map
            for (int y = 0; y < maxBoundaries.y; y++)
            {
                byte[] slice = (byte[])properties[y.ToString()];

                for (int x = 0; x < maxBoundaries.x; x++)
                {
                    for (int z = 0; z < maxBoundaries.z; z++)
                    {
                        if (slice[MapSizeXZ * x + z] > highestHealthBlocks)
                            Debug.Log("Data's corrupted");
                        else if (slice[MapSizeXZ * x + z] == 0)
                            continue;

                        InstantiateBlockFromHealth(x, y, z, slice[MapSizeXZ * x + z]);
                    }
                }
            }
        }


    }
    private void InstantiateBlockFromBool(int x, int y, int z, bool IsClumpBlock)
    {
        if (usingBlockWithHealth)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            if (!blocks.ContainsKey(pos))
            {
                BlockWithHealth blockWHealth = null;
                if (IsClumpBlock)
                {
                    blockWHealth = Instantiate(blockWithHealthClump, new Vector3(x, y, z), quaternion.identity);
                    blockWHealth.InitializeBlockWithHealth(BlockHealthClump);
                }
                else
                {
                    blockWHealth = Instantiate(blockWithHealth, new Vector3(x, y, z), quaternion.identity);
                    blockWHealth.InitializeBlockWithHealth(BlockHealth);
                }
                blockWHealth.name = $"Block ({x}, {y}, {z})";
                blockWHealth.transform.SetParent(transform);

                blocks.Add(new Vector3Int(x, y, z), blockWHealth);
            }
            if (!blocksGroundFloor.ContainsKey(pos))
            {
                if (y <= GroundHeight)
                {
                    blocksGroundFloor.Add(new Vector3Int(x, y, z), blocks[pos]);
                }
            }
        }
        else
        {
            GameObject block = Instantiate(unitBlock, new Vector3(x, y, z), quaternion.identity);
            block.name = $"Block ({x}, {y}, {z})";
            block.transform.SetParent(transform);
        }
    }

    private void InstantiateBlockFromHealth(int x, int y, int z, int health)
    {
        if (usingBlockWithHealth)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            if (!blocks.ContainsKey(pos))
            {
                BlockWithHealth blockWHealth = null;
                if (health == BlockHealthClump)
                {
                    blockWHealth = Instantiate(blockWithHealthClump, new Vector3(x, y, z), quaternion.identity);
                    blockWHealth.InitializeBlockWithHealth(BlockHealthClump);
                }
                else
                {
                    blockWHealth = Instantiate(blockWithHealth, new Vector3(x, y, z), quaternion.identity);
                    blockWHealth.InitializeBlockWithHealth(BlockHealth);
                }
                blockWHealth.name = $"Block ({x}, {y}, {z})";
                blockWHealth.transform.SetParent(transform);
                blocks.Add(new Vector3Int(x, y, z), blockWHealth);
                if (!blocksGroundFloor.ContainsKey(pos))
                {
                    if (y <= GroundHeight)
                    {
                        blocksGroundFloor.Add(new Vector3Int(x, y, z), blocks[pos]);
                    }
                }
            }
        }
        else
        {
            GameObject block = Instantiate(unitBlock, new Vector3(x, y, z), quaternion.identity);
            block.name = $"Block ({x}, {y}, {z})";
            block.transform.SetParent(transform);
        }
    }

    private void Update()
    {
        // Timer for updating room properties
        if (updateTimer > 0f)
            updateTimer -= Time.deltaTime;
        else if (roomDirty && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Room properties updated");
            UpdatePUNRoomProperties();
            updateTimer = 5f;

            roomDirty = false;
        }
    }

    // Callback when master client is switched
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master client changed, syncing map data to the new master client");

        SetMapState(newMasterClient);
    }

    private void UpdatePUNRoomProperties()
    {
        // Not implemented yet
        // throw new NotImplementedException();
    }

    private void GenerateGrid()
    {
        // Generate a grid of blocks for the map
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    if (y > GroundHeight - 1)
                    {
                        if (x < WallThickness || x > mapSize.x - WallThickness - 1 || z < WallThickness || z > mapSize.z - WallThickness - 1)
                        {
                            Vector3Int pos = new Vector3Int(x, y, z);

                            InstantiateBlockFromBool(x, y, z, false);
                            mapState[pos.x, pos.y, pos.z] = BlockHealth;
                        }
                    }
                    else
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        InstantiateBlockFromBool(x, y, z, false);
                        mapState[pos.x, pos.y, pos.z] = BlockHealth;
                    }
                }
            }
        }

        if (GenerateClumps)
        {
            for (int i = 0; i < Random.Range(minClumpAmount, maxClumpAmount); i++)
            {
                GenerateClump();
            }
        }
    }
    private void GenerateClump()
    {
        //generateClumps
        int clumpSizeX = Random.Range(minclumpSize, maxclumpSize);
        int clumpX = Random.Range(WallThickness, MapSizeXZ - WallThickness - clumpSizeX);
        int clumpSizeZ = Random.Range(minclumpSize, maxclumpSize);
        int clumpZ = Random.Range(WallThickness, MapSizeXZ - WallThickness - clumpSizeZ);
        int clumpSizeY = Random.Range(minclumpSize, maxclumpSize);
        int clumpY = Random.Range(GroundHeight, MapSizeY - ClumpsLowerThanWalls - 1 - clumpSizeY);

        for (int y = clumpY; y < clumpY + clumpSizeY; y++)
        {
            for (int x = clumpX; x < clumpX + clumpSizeX; x++)
            {
                for (int z = clumpZ; z < clumpZ + clumpSizeZ; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    InstantiateBlockFromBool(x, y, z, true);
                    mapState[pos.x, pos.y, pos.z] = BlockHealthClump;
                }
            }
        }


    }

    private void ZeroMap()
    {
        // Initialize the rest of the map to zero
        for (int y = 0; y < mapState.GetUpperBound(1); y++)
        {
            for (int x = 0; x < mapState.GetUpperBound(0); x++)
            {
                for (int z = 0; z < mapState.GetUpperBound(2); z++)
                {
                    if (mapState[x, y, z] > highestHealthBlocks)
                    {
                        mapState[x, y, z] = 0;
                    }
                }
            }
        }
    }

    private void SetPunRoomProperties()
    {
        // Set Photon room properties based on map state
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        properties.Add("xMax", mapState.GetUpperBound(0));
        properties.Add("yMax", mapState.GetUpperBound(1));
        properties.Add("zMax", mapState.GetUpperBound(2));

        for (int y = 0; y < mapState.GetUpperBound(1); y++)
        {
            byte[] slice = new byte[MapSizeXZ * MapSizeXZ];

            for (int x = 0; x < mapState.GetUpperBound(0); x++)
            {
                for (int z = 0; z < mapState.GetUpperBound(2); z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    slice[MapSizeXZ * x + z] = (byte)mapState[x, y, z];
                }
            }
            properties.Add(y.ToString(), slice);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        Debug.Log("Room properties set.");
    }

    // Set map state for a specific player
    void SetMapState(Player targetPlayer)
    {
        view.RPC("SetMapStateRPC", targetPlayer);
    }

    // RPC call to set map state
    [PunRPC]
    void SetMapStateRPC()
    {
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;

        for (int y = 0; y < mapState.GetUpperBound(1); y++)
        {
            byte[] slice = (byte[])properties[y.ToString()];

            for (int x = 0; x < mapState.GetUpperBound(0); x++)
            {
                for (int z = 0; z < mapState.GetUpperBound(2); z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    mapState[x, y, z] = slice[MapSizeXZ * x + z];
                }
            }
        }
        Debug.Log("Set Map State Complete");
    }

    // Destroy a block at a given position
    public void DamageBlock(Vector3 transformPosition, int damage)
    {
        if (usingBlockWithHealth)
        {
            view.RPC("DamageBlockRPC", RpcTarget.All, transformPosition, damage);
        }
        else
        {
            view.RPC("DestroyBlockRPC", RpcTarget.All, transformPosition);
        }

    }
    // RPC call to destroy a block
    [PunRPC]
    void DamageBlockRPC(Vector3 pos, int damage)
    {
        // Transform blockT = null;

        Vector3Int vector3Int = Vector3Int.RoundToInt(pos);
        if (blocks.TryGetValue(vector3Int, out BlockWithHealth foundBlock))
        {

        }
        // // Find the block at the given position
        // foreach (Transform child in blockHolder)
        // {
        //     if (child.position == pos)
        //     {
        //         blockT = child;
        //         break;
        //     }
        // }

        if (foundBlock != null)
        {
            // BlockWithHealth block = blockT.gameObject.GetComponent<BlockWithHealth>();
            Vector3 intPos = Vector3Int.FloorToInt(pos);

            if (foundBlock.TakeDamageAndCheckIfDead(damage))
            {
                blocks.Remove(vector3Int);

                if (blocksGroundFloor.ContainsKey(vector3Int))
                {
                    if (vector3Int.y <= GroundHeight)
                    {
                        blocksGroundFloor.Remove(vector3Int);
                    }
                }

                Destroy(foundBlock.gameObject);
                MapGenerator.instance.EditMapState(intPos, 0);
            }
            else
            {
                MapGenerator.instance.EditMapState(intPos, foundBlock.GetCurrentHealth());
            }
        }
    }

    public void DestroyBlock(Vector3 pos)
    {
        view.RPC("DestroyBlockRPC", RpcTarget.All, pos);
    }

    // RPC call to destroy a block
    [PunRPC]
    void DestroyBlockRPC(Vector3 pos)
    {
        Transform blockT = null;

        // Find the block at the given position
        foreach (Transform child in blockHolder)
        {
            if (child.position == pos)
            {
                blockT = child;
                break;
            }
        }

        if (blockT != null)
        {
            GameObject block = blockT.gameObject;
            Destroy(block);

            Vector3 intPos = Vector3Int.FloorToInt(pos);

            // Destroy the block in the map state
            MapGenerator.instance.EditMapState(intPos, 0);
        }
    }

    // Edit map state for a given position
    private void EditMapState(Vector3 position, int state)
    {
        //Debug.Log(position);
        view.RPC("EditMapStateRPC", RpcTarget.MasterClient, position, state);
    }

    // RPC call to edit map state
    [PunRPC]
    void EditMapStateRPC(Vector3 intPos, int state)
    {
        mapState[(int)intPos.x, (int)intPos.y, (int)intPos.z] = state;
    }


    public void startAutoDestroyBlocks()

    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RemoveBlockRoutine(AutoDestroyBlocks));
        }
    }

    public void stopautoDestroyBlocks()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StopCoroutine(RemoveBlockRoutine(AutoDestroyBlocks));
        }
        
    }
    IEnumerator RemoveBlockRoutine(bool destroyblocks)
    {
        while (destroyblocks && blocksGroundFloor.Count > 0)
        {
            yield return new WaitForSeconds(tickInterval);
            for (int i = 0; i < blocksPerTick; i++)
            {
                Removeblock();
            }
        }
    }

    public void Removeblock()
    {
        KeyValuePair<Vector3Int, BlockWithHealth> randomKVP = GetRandomKeyValuePair(blocksGroundFloor);

        DamageBlock(randomKVP.Key, damagePerTick);
    }

    KeyValuePair<Vector3Int, BlockWithHealth> GetRandomKeyValuePair(Dictionary<Vector3Int, BlockWithHealth> dictionary)
    {
        // Convert the dictionary to an array of KeyValuePair objects
        KeyValuePair<Vector3Int, BlockWithHealth>[] kvpArray = new KeyValuePair<Vector3Int, BlockWithHealth>[dictionary.Count];
        int index = 0;
        foreach (var kvp in dictionary)
        {
            kvpArray[index] = kvp;
            index++;
        }

        // Get a random index
        int randomIndex = Random.Range(0, dictionary.Count);

        // Return the random key-value pair
        return kvpArray[randomIndex];
    }
    public void SetRoomDirty()
    {
        view.RPC("SetRoomDirtyRPC", RpcTarget.MasterClient);
    }

    [PunRPC]
    void SetRoomDirtyRPC()
    {
        roomDirty = true;
    }
}
