using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName ="VoxelTUT/Biome Attributes")]
public class BiomeAttribute : ScriptableObject
{
    public string biomeName;
    public int minSolidGroundHeight;
    public int maxTerrainHeight;
    public float terrainScale;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    //public string lodeName;
    //public ushort blockI;
    public World.Block block;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}
