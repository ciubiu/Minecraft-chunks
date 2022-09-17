using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform player;
    public Vector3Int spawnPosition;
    private int tempHeight = 40;
    
    [Header("World")]
    public int seed;
    public BiomeAttribute biome; 

    public Material material;
    public BlockType[] blockTypes;

    readonly int worldX = VoxelData.WorldWidthInChunks;
    readonly int worldY = VoxelData.WorldHeightInChunks;
    readonly int worldZ = VoxelData.WorldDepthInChunks;

    Chunk[,,] worldChunk3DArray;
    List<ChunkCoord> activeChunksCoord = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    bool isCreatingChunks;

    public GameObject debugScreen;

    public enum Block : ushort
    {
        Air,
        Bedrock,
        Stone,
        Grass,
        Dirt,
        Sand,
        Coal,
        Planks,
        Log,
        Gobble,
        Bricks,
        Furnace
    }

    private void Start()
    {
        PlayerSpawnPosition();

        Random.InitState(seed);
        worldChunk3DArray = new Chunk[worldX, worldY, worldZ];
        MakeWorld();

    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine("CreateChunks");
        
        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    void PlayerSpawnPosition()
    {
        // distance in chunks
        int viewDistance = VoxelData.ViewDistranceInChunks;

        int spawnx = Random.Range(0, VoxelData.WorldWidthInChunks - 2 * viewDistance) + viewDistance;
        int spawny = VoxelData.WorldHeightInChunks;
        int spawnz = Random.Range(0, VoxelData.WorldDepthInChunks - 2 * viewDistance) + viewDistance;

        // lets the spawn position be in chunks 
        spawnPosition = new Vector3Int(spawnx, spawny, spawnz);
       
        // global world coordinates
        player.position = new Vector3(spawnx * VoxelData.ChunkWidth, spawny * VoxelData.ChunkHeight - tempHeight, spawnz * VoxelData.ChunkDepth);
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

    }

    void MakeWorld()
    {
        // we should generate chunk only around player
        for (int x = spawnPosition.x - VoxelData.ViewDistranceInChunks; x < spawnPosition.x + VoxelData.ViewDistranceInChunks ; x++)        {
            for (int y = 0; y < worldChunk3DArray.GetLength(1); y++)            {
                for (int z = spawnPosition.z - VoxelData.ViewDistranceInChunks; z < spawnPosition.z + VoxelData.ViewDistranceInChunks; z++)
                {
                    worldChunk3DArray[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this, true);
                    activeChunksCoord.Add(new ChunkCoord(x, y, z));
                }
            }
        }
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            worldChunk3DArray[chunksToCreate[0].x, chunksToCreate[0].y, chunksToCreate[0].z].InitChunk();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    
    void CheckViewDistance()
    {
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunksCoord);
        activeChunksCoord.Clear();    // we will populate it again below

        for (int x = playerChunkCoord.x - VoxelData.ViewDistranceInChunks; x < playerChunkCoord.x + VoxelData.ViewDistranceInChunks; x++)        {
            for (int y = 0; y < worldChunk3DArray.GetLength(1); y++)            {
                for (int z = playerChunkCoord.z - VoxelData.ViewDistranceInChunks; z < playerChunkCoord.z + VoxelData.ViewDistranceInChunks; z++)
                {
                    var currentChunk = new ChunkCoord(x, y, z);
                    // 'playerChunkCoord' gives IndexOutOFRangeException, so make new ChunkCoord
                    if (IsChunkInWorld(currentChunk))
                    {
                        // chunk never added to array
                        if (worldChunk3DArray[x, y, z] == null)
                        {
                            worldChunk3DArray[x, y, z] = new Chunk(currentChunk, this, false);
                            chunksToCreate.Add(currentChunk);
                        }
                        // chunk in array, but not active
                        else if (!worldChunk3DArray[x, y, z].isActive)
                        {
                            worldChunk3DArray[x, y, z].isActive = true;
                            activeChunksCoord.Add(new ChunkCoord(x,y,z));
                        }
                        // chunk in array and active
                        else
                        {
                            activeChunksCoord.Add(currentChunk);
                        }
                        
                    }

                    // does it go through like 10x to remove every each one???
                    //Debug.Log("pac: "+ previouslyActiveChunks.Count);
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        if (previouslyActiveChunks[i].Equals(currentChunk))
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                        //Debug.Log("count: "+ i);
                    }
                }
            }
        }

        foreach (ChunkCoord item in previouslyActiveChunks)
        {
            worldChunk3DArray[item.x, item.y, item.z].isActive = false;
        }
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);
        return new ChunkCoord(x, y, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);
        return  worldChunk3DArray[x, y, z];
    }


    //public bool BlockExists(float _x, float _y, float _z)
    //{
    //    int x = Mathf.FloorToInt(_x);
    //    int y = Mathf.FloorToInt(_y);
    //    int z = Mathf.FloorToInt(_z);

    //    // find the chunk coords,  
    //    // because we search just that junk inside 'worldChunks' 3darray for optimazation
    //    int cx = x / VoxelData.ChunkWidth;
    //    int cy = y / VoxelData.ChunkHeight;
    //    int cz = z / VoxelData.ChunkDepth;

    //    // we need the value of the block in that chunk
    //    x -= cx * VoxelData.ChunkWidth;  // example: 278 - 13*20 = 18
    //    y -= cy * VoxelData.ChunkHeight; // example: 102 - 0*120 = 102
    //    z -= cz * VoxelData.ChunkDepth;  // example: 53  - 2*20  = 13

    //    // is chunk (chunkMap typeOf Chunk) in worldChunks solid
    //    //return blockTypes[ worldChunks[cx, cy, cz].blockMap[x,y,z] ].isSolid;
    //    if (blockTypes[worldChunks[cx, cy, cz].blockMap[x, y, z]].isSolid)
    //        return true;
    //    else
    //        return false;
    //}


    public bool CheckForBlock(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsBlockInWorld(pos))
            return false;

        if (worldChunk3DArray[thisChunk.x, thisChunk.y, thisChunk.z] != null && worldChunk3DArray[thisChunk.x, thisChunk.y, thisChunk.z].isBlockMapGenerated)
            return blockTypes[ worldChunk3DArray[thisChunk.x, thisChunk.y, thisChunk.z].GetBlockFromGlobalVector3(pos)].isSolid;

        return blockTypes[GetBlockType(pos)].isSolid;
    }


    public ushort GetBlockType (Vector3 bpos)
    {
        int yPos = Mathf.FloorToInt(bpos.y);
        /* IMMUTABLE PASS*/

        if (!IsBlockInWorld(bpos))             
            return (ushort)Block.Air;

        if (yPos < 1)
            return (ushort)Block.Bedrock;

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(biome.maxTerrainHeight * Noise.Get2DPerlin( new Vector2(bpos.x, bpos.z), 500, 0.25f) ) + biome.minSolidGroundHeight;
        ushort blockValue = 0;

        /*  FIRST PASS  */

        if (yPos == terrainHeight)
            //return (ushort)Block.Grass;
            blockValue = (ushort)Block.Grass;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            //return (ushort)Block.Dirt;
            blockValue = (ushort)Block.Dirt;
        else if (yPos > terrainHeight)
            return (ushort)Block.Air;
        else
            //return (ushort)Block.Stone;
            blockValue = (ushort)Block.Stone;

        /*  SECOND PASS  */

        if (blockValue == (ushort)Block.Stone)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(bpos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        blockValue = (ushort)lode.block;
                    }

            }
        }

        return blockValue;


        //else if (bpos.y == VoxelData.worldHeightInChunks * VoxelData.chunkHeight - 1)
        //{
        //    float tempNoise = Noise.Get2DPerlin(new Vector2(bpos.x, bpos.z), 0, 0.1f);
        //    if (tempNoise < 0.5f)
        //        return (ushort)Block.Grass;
        //    else
        //        return (ushort)Block.Sand;
        //}
        //else
        //    return (ushort)Block.Stone;
    }

    public bool IsBlockInWorld(Vector3 bpos)
    {
        int WorldWidthInBlocks = worldX * VoxelData.ChunkWidth;
        int WorldHeightInBlocks = worldY * VoxelData.ChunkHeight;
        int WorldDepthInBlocks = worldZ * VoxelData.ChunkDepth;

        if (bpos.x >= 0 && bpos.x < WorldWidthInBlocks &&
             bpos.y >= 0 && bpos.y < WorldHeightInBlocks &&
             bpos.z >= 0 && bpos.z < WorldDepthInBlocks)
            return true;
        else
            return false;
    }


    // bool IsChunkInWorld (ChunkCoord coord) vid.04 23:30
    // are they world or chunk coords??
    // Answer: they should be chunk index coordinates, like 5th left and 2nd forward chunk
    bool IsChunkInWorld(ChunkCoord coord)
    {
        int cx = coord.x;
        int cy = coord.y;
        int cz = coord.z;

        if (cx >= 0 && cx < worldX && cy >= 0 && cy < worldY  && cz >= 0 && cz < worldZ )
            return true;
        else
            return false;
    }
    
}

[System.Serializable]
public class BlockType
{
    public string type;  // In Inspector, if string is first value, you see name not like Element0,...
    public bool isSolid;
    public Sprite icon;

    //[Header("Texture Values")]
    // front, back, left, right, down, up
    public int upFace;
    public RectInt sideFBLR;
    //private int frontFace;
    //private int backFace ;
    //private int leftFace; 
    //private int rightFace;
    public int downFace;

    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return sideFBLR.x;  //front face
            case 1:
                return sideFBLR.y;  //back face
            case 2:
                return sideFBLR.width;  //left face
            case 3:
                return sideFBLR.height;  // right face
            case 4:
                return downFace;
            case 5:
                return upFace;
            default:
                Debug.Log("Error in GetTextureID, invalid face index");
                return 0;
        }
    }
}
