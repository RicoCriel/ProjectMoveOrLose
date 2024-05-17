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

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviourPunCallbacks
{
    public static MapGenerator instance; // Singleton instance of MapGenerator

    public bool GenerateMapFromCurrentRoomProperties = false;


    [Header("BlocksWithHealth")]
    public BlockWithHealth blockWithHealth;
    public BlockWithHealth blockWithHealthClump;
    public int BlockHealth = 4;
    public int BlockHealthClump = 8;


    public Transform blockHolder; // Parent object to hold all the blocks


    private PhotonView view; // Reference to the PhotonView component

    private LayerMask blockLayer; // Layer mask for blocks

    private Vector3 mapSize; /*= new Vector3(29, 3, 29);*/ // Size of the map

    [Header("BaseMapGen")]
    public int MapSizeXZ = 30; // Size of the map
    public int MapSizeY = 50; // Size of the map


    [HideInInspector] public Vector2Int xBoundary /*= new Vector2Int(1, 30)*/; // X boundaries of the map
    [HideInInspector] public Vector2Int yBoundary /*= new Vector2Int(1, 50)*/; // Y boundaries of the map
    [HideInInspector] public Vector2Int zBoundary /*= new Vector2Int(1, 30)*/; // Z boundaries of the map

    public int[,,] mapState; // 3D array to store the state of each block in the map

    private float updateTimer = 5f; // Timer for updating room properties

    [HideInInspector] public bool roomDirty = false; // Flag to indicate if room properties need updating


    Dictionary<Vector3Int, BlockWithHealth> blocks = new Dictionary<Vector3Int, BlockWithHealth>();


    private int highestHealthBlocks;

    [Header("buttons")]
    public bool GenerateWallsButton;
    public bool DestroyWallsButton;

    public Vector3 MapCenter = new Vector3(15, 0, 15);

    public Vector3[] CubeDirections = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

    private void Awake()
    {
        highestHealthBlocks = Mathf.Max(BlockHealth, BlockHealthClump);

        instance = this; // Set the instance to this object

        mapSize = new Vector3(MapSizeXZ - 1, MapSizeY, MapSizeXZ - 1); // Set the map size
        xBoundary = new Vector2Int(1, MapSizeXZ); // Set the x boundaries
        yBoundary = new Vector2Int(1, MapSizeY); // Set the y boundaries
        zBoundary = new Vector2Int(1, MapSizeXZ); // Set the z boundaries

        MapCenter = new Vector3(MapSizeXZ / 2f, MapSizeY / 2f, MapSizeXZ / 2f);

        //
        // xBoundary = new Vector2Int((-MapSizeXZ /2) - 1, (MapSizeXZ/2) + 1); // Set the x boundaries
        // yBoundary = new Vector2Int((-MapSizeY /2 -1), (MapSizeY/2) + 1); // Set the y boundaries
        // zBoundary = new Vector2Int((-MapSizeXZ /2) -1 , (MapSizeXZ/2) +1); // Set the z boundaries

        view = GetComponent<PhotonView>(); // Get the PhotonView component attached to this object
        blockLayer = LayerMask.GetMask("Block"); // Get the layer mask for blocks
        blockLayer = ~blockLayer; // Invert the layer mask
    }

    private void OnDrawGizmos()
    {
        var xBoundaryn = new Vector2(-0.5f, MapSizeXZ - 1.5f); // Set the x boundaries
        var yBoundaryn = new Vector2(-0.5f, MapSizeY - 0.5f); // Set the y boundaries
        var zBoundaryn = new Vector2(-0.5f, MapSizeXZ - 1.5f); // Set the z boundaries
        // Set the color of the gizmos
        Gizmos.color = Color.green;

        // Define the boundaries
        float xMin = xBoundaryn.X;
        float xMax = xBoundaryn.Y;
        float yMin = yBoundaryn.X;
        float yMax = yBoundaryn.Y;
        float zMin = zBoundaryn.X;
        float zMax = zBoundaryn.Y;

        // Define the 8 corners of the boundary box
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(xMin, yMin, zMin);
        corners[1] = new Vector3(xMax, yMin, zMin);
        corners[2] = new Vector3(xMax, yMin, zMax);
        corners[3] = new Vector3(xMin, yMin, zMax);
        corners[4] = new Vector3(xMin, yMax, zMin);
        corners[5] = new Vector3(xMax, yMax, zMin);
        corners[6] = new Vector3(xMax, yMax, zMax);
        corners[7] = new Vector3(xMin, yMax, zMax);

        // Draw the bottom square
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);

        // Draw the top square
        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[6]);
        Gizmos.DrawLine(corners[6], corners[7]);
        Gizmos.DrawLine(corners[7], corners[4]);

        // Draw the vertical lines
        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);
    }

    private void Start()
    {
        mapState = new int[xBoundary.y, yBoundary.y, zBoundary.y]; // Initialize the map state array
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (PhotonNetwork.IsMasterClient && !properties.ContainsKey("xMax"))
        {
            readOutGrid();
            ZeroMap(); // Set the rest of the map to zero
            SetPunRoomProperties(); // Set Photon room properties
        }
        else if (GenerateMapFromCurrentRoomProperties)
        {
            Debug.Log("Not master client, getting room data...");

            // Retrieve room data from Photon custom properties


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
        else
        {
            readOutGrid();
        }


    }

    public int WallThickness = 1; // Thickness of the walls
    public int GroundHeight = 1; // Height of the ground

    [ExecuteInEditMode]
    public void GenerateWalls()
    {
        mapSize = new Vector3(MapSizeXZ - 1, MapSizeY, MapSizeXZ - 1); // Set the map size
        xBoundary = new Vector2Int(1, MapSizeXZ); // Set the x boundaries
        yBoundary = new Vector2Int(1, MapSizeY); // Set the y boundaries
        zBoundary = new Vector2Int(1, MapSizeXZ); // Set the z boundaries

        DestroyWalls();

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    if (y > WallThickness - 1 && y < mapSize.y - WallThickness)
                    {
                        if (x < WallThickness || x > mapSize.x - WallThickness - 1 || z < WallThickness || z > mapSize.z - WallThickness - 1)
                        {
                            generateblock(x, y, z);
                        }
                    }
                    else
                    {
                        generateblock(x, y, z);
                    }

                }
            }
        }
    }

    public void DestroyWalls()
    {
        for (int i = blockHolder.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(blockHolder.transform.GetChild(i).gameObject);
        }
    }
    private void generateblock(int x, int y, int z)
    {

        BlockWithHealth blockWHealth = Instantiate(blockWithHealth, new Vector3(x, y, z), quaternion.identity);
        // blockWHealth.InitializeBlockWithHealth(BlockHealth);

        blockWHealth.name = $"Block ({x}, {y}, {z})";
        blockWHealth.transform.SetParent(blockHolder);

        #if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(blockWHealth.gameObject, "Instantiate Object");
        UnityEditor.EditorUtility.SetDirty(blockWHealth.gameObject);
#endif
    }


    private void InstantiateBlockFromHealth(int x, int y, int z, int health)
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
            blockWHealth.transform.SetParent(blockHolder);
            blocks.Add(new Vector3Int(x, y, z), blockWHealth);

        }


    }

    private void Update()
    {
         #if UNITY_EDITOR
        if (GenerateWallsButton)
        {
            GenerateWalls();
            GenerateWallsButton = false;
        }

        if (DestroyWallsButton)
        {
            DestroyWalls();
            DestroyWallsButton = false;
        }
#endif

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

    public void readOutGrid()
    {
        BlockWithHealth[] FoundBlocks = FindObjectsOfType<BlockWithHealth>();
        foreach (BlockWithHealth block in FoundBlocks)
        {
            Vector3Int pos = Vector3Int.RoundToInt(block.transform.position);
            if (block.blockType == BlockType.Chunk)
            {
                mapState[pos.x, pos.y, pos.z] = BlockHealthClump;
                block.InitializeBlockWithHealth(BlockHealthClump);

            }
            else if (block.blockType == BlockType.Normal)
            {
                mapState[pos.x, pos.y, pos.z] = BlockHealth;
                block.InitializeBlockWithHealth(BlockHealth);
            }

            block.name = $"Block ({pos.x}, {pos.y}, {pos.z})";
            block.transform.SetParent(transform);

            blocks.Add(new Vector3Int(pos.x, pos.y, pos.z), block);
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
        view.RPC("DamageBlockRPC", RpcTarget.All, transformPosition, damage);
    }

    // RPC call to destroy a block
    [PunRPC]
    void DamageBlockRPC(Vector3 pos, int damage)
    {


        Vector3Int vector3Int = Vector3Int.RoundToInt(pos);
        if (blocks.TryGetValue(vector3Int, out BlockWithHealth foundBlock))
        {

        }


        if (foundBlock != null)
        {
            // BlockWithHealth block = blockT.gameObject.GetComponent<BlockWithHealth>();
            Vector3 intPos = Vector3Int.FloorToInt(pos);

            if (foundBlock.TakeDamageAndCheckIfDead(damage))
            {
                blocks.Remove(vector3Int);

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

    public void SetRoomDirty()
    {
        view.RPC("SetRoomDirtyRPC", RpcTarget.MasterClient);
        // Debug.Log(roomDirty);
    }

    [PunRPC]
    void SetRoomDirtyRPC()
    {
        roomDirty = true;
    }




    public Vector3 GetFreePosition()
    {
        throw new NotImplementedException();
    }
}
