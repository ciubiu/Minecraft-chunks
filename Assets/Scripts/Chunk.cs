using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;
    private World _world;
    private GameObject chunkObject;
    private bool _isActive;
    public bool isBlockMapGenerated;

    private Mesh _mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    private int _triangleIndex = 0;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    private MeshCollider _meshCollider;

    // because I can not put field values here for initializing, I need to use static type from VoxelData:
    public ushort[,,] blockMap = new ushort[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkDepth];

    public Chunk(ChunkCoord getCoord, World getWorld, bool generateOnLoad)
    {
        coord = getCoord;
        _world = getWorld;
        isActive = true;

        if (generateOnLoad)
            InitChunk();

        
    }

    public void InitChunk()
    {
        chunkObject = new GameObject();
        chunkObject.transform.SetParent(_world.transform);
        chunkObject.transform.position = new Vector3(
                                                    coord.x * VoxelData.ChunkWidth,
                                                    coord.y * VoxelData.ChunkHeight,
                                                    coord.z * VoxelData.ChunkDepth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + ", " + coord.z;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        _mesh = meshFilter.mesh;
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = _world.material;
        _meshCollider = chunkObject.AddComponent<MeshCollider>();


        GenerateChunkMap(); // set all indices true/false
        PopulateChunk();
        

        _meshCollider.sharedMesh = meshFilter.mesh;
    }


    void GenerateChunkMap()
    {
        for (int z = 0; z < VoxelData.ChunkDepth; z++)        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)            {
                for (int y = 0; y < VoxelData.ChunkHeight; y++)                
                {
                    // getting global pos with adding Position
                    blockMap[x, y, z] = _world.GetBlockType(new Vector3(x, y, z) + Position);

                }
            }
        }

        isBlockMapGenerated = true;
    }


    void PopulateChunk()
    {
        ClearMeshData();
        for (int z = 0; z < VoxelData.ChunkDepth; z++)        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)            {
                for (int y = 0; y < VoxelData.ChunkHeight; y++)                
                {
                    if (_world.blockTypes[blockMap[x, y, z]].isSolid)
                    {
                        AddBlockFaces(new Vector3(x, y, z));
                    }
                    
                }
            }
        }
        DrawChunk();
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        _triangleIndex = 0;
        uvs.Clear();
    }

    void AddBlockFaces(Vector3 pos)
    {
        for (int f = 0; f < 6; f++)  // cube have 6 faces
        {
            // if it is an outer face
            // false also then next position is "over the edge"
            if (!FaceHasNeighbor(pos + VoxelData.FaceCheck[f]) )  // && blocktype is not AIR
            {

                // ushort - we can have more than 256 textures
                // pos.x is float, but an array index is int
                ushort blockId = blockMap[(int)pos.x, (int)pos.y, (int)pos.z];
                for (int v = 0; v < 4; v++)
                {
                    vertices.Add(VoxelData.CubeVertices[f, v] + pos);
                    //uvs.Add(VoxelData.faceUVs[v]);
                }

                //AddTexture(14);
                AddTexture(_world.blockTypes[blockId].GetTextureId(f));

                // It is recommended to assign a triangle array after assigning the vertex array, in order to avoid out of bounds errors.
                // 
                // Triangle vertices have to increase every step
                // or all the voxel triangles will be draw in same place
                // you can have thousands of triangles but they are stacked in one place if you use without vertriIndex
                //
                // Face Triangles  { 0,1,2,0,2,3 }
                // why triangles.Add(vertriIndex + VoxelData.voxelTriangles[f, 1]); is not working here??
                // triangle drawing pattern is same on every face. We are only generating vertices to outer faces and
                // remember triangle index is refering to vertex in array. So first six triangle indices refer to first 4 vertices
                // and 4 vertices is one face. For second face, for ever it is, we just say from there to start drawing triangle...
                // 
                // cant I add them like triangles.AddRange({0, 1, 2, 0, 2, 3}); or I need method for that??
                triangles.Add(_triangleIndex + 0);
                triangles.Add(_triangleIndex + 1);
                triangles.Add(_triangleIndex + 2);
                triangles.Add(_triangleIndex + 0);
                triangles.Add(_triangleIndex + 2);
                triangles.Add(_triangleIndex + 3);
                // we need new indices for new voxel
                _triangleIndex += 4;
            }
        }
    }


    bool FaceHasNeighbor(Vector3 pos)
    {
        // converting from float to int is not 100% accurate, so
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsInsideChunk(x, y, z))
        {
            // *******************************
            // this calculated position check does the trick to erase faces between chunks
            // *******************************
            // pos is local chunk coords, GetBlockType is global world coordinates
            //
            /* return _world.blockTypes[_world.GetBlockType(pos + Position)].isSolid; */

            return _world.CheckForBlock(pos + Position);
        }
        return _world.blockTypes[blockMap[x, y, z]].isSolid;
    }

    public ushort GetBlockFromGlobalVector3 (Vector3 pos)
    {
        int gx = Mathf.FloorToInt(pos.x);
        int gy = Mathf.FloorToInt(pos.y);
        int gz = Mathf.FloorToInt(pos.z);

        gx -= Mathf.FloorToInt(chunkObject.transform.position.x);
        gy -= Mathf.FloorToInt(chunkObject.transform.position.y);
        gz -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return blockMap[gx, gy, gz];
    }

    void DrawChunk()
    {
        _mesh.Clear();
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.uv = uvs.ToArray();
        _mesh.RecalculateNormals(); // if doing it without uvs, we get black hole material
    }


    // then we move around to de/activate chunks
    public bool isActive 
    {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null)
            chunkObject.SetActive(value);  
        }
    }

    public Vector3 Position
    {
        get { return chunkObject.transform.position; }
    }

    bool IsInsideChunk (int x, int y, int z)
    {
        // these are local chunk coords not global world coordinates
        if (x > 0 || x < VoxelData.ChunkWidth - 1 || y > 0 || y < VoxelData.ChunkHeight - 1 || z > 0 || z < VoxelData.ChunkDepth - 1)
            return false;
        else
            return true;
    }

    public void EditBlock(Vector3 pos, ushort newID)
    {
        int gx = Mathf.FloorToInt(pos.x);
        int gy = Mathf.FloorToInt(pos.y);
        int gz = Mathf.FloorToInt(pos.z);

        gx -= Mathf.FloorToInt(chunkObject.transform.position.x);
        gy -= Mathf.FloorToInt(chunkObject.transform.position.y);
        gz -= Mathf.FloorToInt(chunkObject.transform.position.z);

        // adding new block
        blockMap[gx, gy, gz] = newID;
        
        // update neighbour chunks
        UpdateNeighbourChunks(gx,gy,gz);
        
        PopulateChunk();
    }


    void UpdateNeighbourChunks(int x, int y, int z)
    {
        Vector3 thisBlock = new Vector3(x, y, z);

        for (int f = 0; f < 6; f++)
        {
            Vector3 currentBlock = thisBlock + VoxelData.FaceCheck[f];
            
            if (!IsInsideChunk((int)currentBlock.x, (int)currentBlock.y, (int)currentBlock.z))
            {
                // how we can add general class method to this end???
                _world.GetChunkFromVector3(currentBlock + Position).PopulateChunk();
            }
        }
    }
    
    void AddTexture (int textureId)
    {
        /* Texture indexes
         *     0  1  2  3
         *   -------------
            3| 0| 1|
            2|
            1| 8| 9|10|11|
            0|
        */

        // y = XXX,xxx - XXX is row number and xxx is column coords
        float y = textureId * VoxelData.TextureUvSize;
        // store row number
        int rowIndex = Mathf.FloorToInt(y);
        // turn row number upsidedown, because uv coords starts from down left corner
        float x = y - rowIndex;
        // convert row number to row index (index starts from 0)
        rowIndex = (VoxelData.TexturesInRow - rowIndex) - 1;
        // make it coords in uv map
        y = rowIndex * VoxelData.TextureUvSize;

        // mesh.uv = new Vector2[]; // should be simple way to make normals work
        // add 4 uvs
        uvs.Add(new Vector2(x, y));  // 00
        uvs.Add(new Vector2(x, y + VoxelData.TextureUvSize)); // 01
        uvs.Add(new Vector2(x + VoxelData.TextureUvSize, y + VoxelData.TextureUvSize)); // 11
        uvs.Add(new Vector2(x + VoxelData.TextureUvSize, y)); // 10
    }
}

public class ChunkCoord
{
    public int x;
    public int y;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int _x = Mathf.FloorToInt(pos.x);
        int _y = Mathf.FloorToInt(pos.y);
        int _z = Mathf.FloorToInt(pos.z);

        x = _x / VoxelData.ChunkWidth;
        y = _y / VoxelData.ChunkHeight;
        z = _z / VoxelData.ChunkDepth;
    }

    public bool Equals (ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.y == y && other.z == z)
            return true;
        else
            return false;
    }
}
