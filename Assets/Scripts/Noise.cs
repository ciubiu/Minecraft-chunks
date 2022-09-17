using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float Get2DPerlin (Vector2 pos, float offset, float scale)
    {
        // if you pass it a whole number you get back a whole number
        // and if you change chunk width and depth, you will get higher or low hills than before
        return Mathf.PerlinNoise((pos.x + 0.1f) / VoxelData.ChunkWidth * scale + offset,
                                 (pos.y + 0.1f) / VoxelData.ChunkDepth * scale + offset);
    }

    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        // https://www.youtube.com/watch?v=Aga0TBJkchM Carpilot on YouTube

        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
            return true;
        else
            return false;

    }

}
