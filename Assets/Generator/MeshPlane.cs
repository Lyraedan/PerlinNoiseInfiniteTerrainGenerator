using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Zel
/// </summary>
public class MeshPlane : MonoBehaviour
{
    public float x, y, z;

    public static Vector3 tileSize = new Vector3(10, 0, 10);

    public Vector3[] vertices = new Vector3[4];
    public int[] indices = new int[6];
    public Vector3[] normals = new Vector3[4];
    public Vector2[] uv = new Vector2[4];

    private Mesh mesh;
    [HideInInspector] public new MeshRenderer renderer;
    public MeshFilter filter;

    /// <summary>
    /// Where is this mesh plane located?
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void SetPosition(float x, float z, float y = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /// <summary>
    /// Build the structure for the mesh, vertices, uvs, normals, indices
    /// </summary>
    public void Build()
    {
        vertices[0] = new Vector3(x, y, z);
        vertices[1] = new Vector3(x + tileSize.x, y, z);
        vertices[2] = new Vector3(x, y, z + tileSize.z);
        vertices[3] = new Vector3(x + tileSize.x, y, z + tileSize.z);

        indices = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };

        uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
    }

    /// <summary>
    /// Colour our mesh based on its vertices height
    /// </summary>
    [Obsolete]
    public void ColourMesh()
    {
        float v0 = Mathf.Lerp(vertices[0].x / Chunk.width, vertices[0].y, vertices[0].z / Chunk.length);
        float v1 = Mathf.Lerp(vertices[1].x / Chunk.width, vertices[1].y, vertices[1].z / Chunk.length);
        float v2 = Mathf.Lerp(vertices[2].x / Chunk.width, vertices[2].y, vertices[2].z / Chunk.length);
        float v3 = Mathf.Lerp(vertices[3].x / Chunk.width, vertices[3].y, vertices[3].z / Chunk.length);

        float interpolation = Mathf.Round(v0 + v1 + v2 + v3);
        Color meshColor = new Color(0, 80, 0);
        // lowest = 25
        // highest = 52
        /*
        if (interpolation < 40)
            meshColor = new Color(80, 80, 0);
        else if (interpolation >= 40 && interpolation < 44)
            meshColor = new Color(0, 150, 0);
        else if (interpolation >= 44 && interpolation < 48)
            meshColor = new Color(0, 120, 120);
        else if (interpolation > 50)
            meshColor = new Color(30, 30, 30);

            */
        //renderer.material.color = meshColor;
    }

    /// <summary>
    /// Build the mesh object and apply it to the mesh filter
    /// </summary>
    public void BuildMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = indices;
        mesh.uv = uv;
        filter.mesh = mesh;
    }

    /// <summary>
    /// Prep our mesh plane with everything it needs for untiy to display it
    /// </summary>
    public void SetupMesh()
    {
        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));

        filter = gameObject.AddComponent<MeshFilter>();
    }

}
