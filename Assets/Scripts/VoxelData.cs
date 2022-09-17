using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 6;  // x - width
    public static readonly int ChunkHeight = 120; // y - height
    public static readonly int ChunkDepth = 6;  // z - depth (this is forward direction in 3D space aka Blue Axes)

    public static readonly int WorldWidthInChunks = 10;
    public static readonly int WorldHeightInChunks = 1;
    public static readonly int WorldDepthInChunks = 10;

    public static int WorldWidthInBlocks => WorldWidthInChunks * ChunkWidth;
    public static int WorldHeightInBlocks => WorldHeightInChunks * ChunkHeight;
    public static int WorldDepthInBlocks => WorldDepthInChunks * ChunkDepth;

    public static readonly int ViewDistranceInChunks = 4; // minimum is (world_width / 2)

    public static readonly int TexturesInRow = 4;
    public static float TextureUvSize => 1f / TexturesInRow;

    public static readonly Vector3[,] CubeVertices = new Vector3[6, 4]
    {
        { new Vector3(0.0f, 0.0f, 0.0f), // South face vertices | z0
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)},

        { new Vector3(1.0f, 0.0f, 1.0f), // North face vertices | z1
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 0.0f, 1.0f)},

        { new Vector3(0.0f, 0.0f, 1.0f), // West face vertices | x0
        new Vector3(0.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 0.0f) },

        { new Vector3(1.0f, 0.0f, 0.0f), // East face vertices | x1
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f)},

        { new Vector3(0.0f, 0.0f, 1.0f), // Down face vertices | y0
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 1.0f)},

        { new Vector3(0.0f, 1.0f, 0.0f),  // Up face vertices | y1 
        new Vector3(0.0f, 1.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 0.0f)}
    };

    // now we dont need it anymore, but is good as triangles reference table
    //public static readonly int[,] voxelTriangles = new int[6, 4]
    //{
    //                    //  { 0,1,2,0,2,3 }
    //    { 0,1,2,3 },    // z0 face with two triangels with indices 0,1,2 and 0,2,3
    //    { 4,5,6,7 },
    //    { 8,9,10,11 },    // x0
    //    { 12,13,14,15 },
    //    { 16,17,18,19 },    // y0
    //    { 20,21,22,23 }
    //};


    // Vector2 on uvs is the texture coordinates from 0 to 1. So min is 0,0 and max is 1,1 and 0.5 is middle of texture
    // better to use (int umin, umax, vmin, vmax) for taking textures from texturemap/atlas
    public static readonly Vector2[] FaceUVs = new Vector2[]
    {
        new Vector2(0f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
        new Vector2(1f, 0f)
    };

    public static readonly Vector3[] FaceCheck = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f), // z0, check rear
        new Vector3(0.0f, 0.0f, 1.0f), // 
        new Vector3(-1.0f, 0.0f, 0.0f), // x0, check left
        new Vector3(1.0f, 0.0f, 0.0f), // 
        new Vector3(0.0f, -1.0f, 0.0f), // y0, check down
        new Vector3(0.0f, 1.0f, 0.0f)  // 
    };

}

/*  Unity default cube have 24 vertices: this is 8x3 direction vertices or 4x6 face vertices. 
 *  This mean every vertex point is facing in 3 directions (x,y,z).
 *  On that way you can set uv-s to every side and on every triangle. 36 uv points is overkill.
 * 
 * 
 * 
 * 
 * 
 */
