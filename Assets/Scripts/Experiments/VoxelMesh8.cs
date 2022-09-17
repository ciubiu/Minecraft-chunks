using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// then you drag VoxelMesh script to object these requirements will be added to object
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelMesh8 : MonoBehaviour
{
    Mesh mesh;

    public readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f), // front vertices from z0 face
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f), // rear vertices from z1 face
        new Vector3(0.0f, 1.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f)
    };

    public static readonly int[] voxelTriangles = new int[]
    {
         0,1,2,0,2,3,    // z0 face with two triangels with indices 0,1,2 and 0,2,3
         7,6,5,7,5,4,
         4,5,1,4,1,0,    // x0
         3,2,6,3,6,7,
         4,0,3,4,3,7,    // y0
         1,5,6,1,6,2 
    };

    private void Awake()
    {
        //These added component only exists during runtime, so you cant add materjal by draging in edit mode to it
        //gameObject.AddComponent<MeshFilter>(); // you can also use [RequireComponent(typeof(MeshFilter))] before class line
        //gameObject.AddComponent<MeshRenderer>();

        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        mesh.Clear();
        mesh.vertices = voxelVertices;
        //mesh.RecalculateNormals(); // if doing it without uvs, we get black hole material
        //mesh.uv = new Vector2[]; // should be simple way to add some way working normals
        // It is recommended to assign a triangle array after assigning the vertex array, in order to avoid out of bounds errors.
        mesh.triangles = voxelTriangles;

        // you can get wakky positioned texture stretched around object
        Vector2[] uvs2 = new Vector2[8] {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 0.0f)
        };

        // - its like a log style texture on cube if cube is made from 8 vertices not from 24 vertices
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
        }

        mesh.uv = uvs;
        Debug.Log($"Vertices {mesh.vertices.Length}");
        Debug.Log($"Triangles {mesh.triangles.Length}");

    }

}
