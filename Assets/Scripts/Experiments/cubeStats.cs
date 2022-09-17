using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeStats : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;

        Debug.Log("CS-Vertices: " + mesh.vertices.Length);
            
        for (int i = 0; i < uvs.Length; i++)
        {
            //uvs[i] = new Vector2(vertices[i].x - 0.5f, vertices[i].z - 0.5f);
            Debug.Log($" {vertices[i].ToString()}");
            
        }
        
        Debug.Log("CS-Triangles: " + mesh.triangles.Length);
        for (int i = 0; i < triangles.Length; i++)
        {
            Debug.Log($" {triangles[i].ToString()}");

        }

        Debug.Log("CS-UVs: " + uvs.Length);
        for (int i = 0; i < uvs.Length; i++)
        {
            Debug.Log($" {uvs[i].ToString()}");

        }
        Debug.Log("CS-UVs: -------------------");
        //mesh.uv = uvs;
    }
}
