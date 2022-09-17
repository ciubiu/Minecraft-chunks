using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitsToBoardsMeshdata : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangle0;

    private void Awake()
    {
        gameObject.AddComponent<MeshFilter>(); // you can also use [RequireComponent(typeof(MeshFilter))] before class line
        gameObject.AddComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        MeshData();
        CreateMesh();
    }

    void MeshData()
    {
        vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) };
        uvs = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        triangle0 = new int[] { 0, 1, 2, 0, 2, 3 }; // faceXY = two triangels with indices 0,1,2 and 0,2,3
    }

    void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs; // we define normals to have lighting effect on them
        // It is recommended to assign a triangle array after assigning the vertex array, in order to avoid out of bounds errors.
        mesh.triangles = triangle0;
    }

}
