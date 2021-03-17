using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    Author: Zel 
     
*/
public class Player : MonoBehaviour
{

    private Vector3 lastPos;

    public float viewSize = 2;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Vector3.Distance(lastPos, transform.position) < 1f) return;
        lastPos = transform.position;
        World.instance.HideChunks();
        GenerateChunkIfWeNeedTo();
    }

    /// <summary>
    /// Generate or Render generated chunks relative to the cameras position in worldspace
    /// </summary>
    public void GenerateChunkIfWeNeedTo()
    {
        float camX = ChunkX();
        float camZ = ChunkZ();
        for (float x = camX - viewSize; x <= camX + viewSize; x++)
        {
            for (float z = camZ - viewSize; z <= camZ + viewSize; z++)
            {
                if (!World.instance.ChunkExists(x, z))
                {
                    World.instance.Generate(x, z);
                } else
                {
                    World.instance.GetChunkAt(x, z).RenderChunk(true);
                }
            }
        }
    }

    /// <summary>
    /// Our camera position in corrolation to the chunk position so if we are in chunk 0, 0 it returns 0, if we are in chunk 1, 0 we are in 1
    /// </summary>
    /// <returns></returns>
    public float ChunkX()
    {
        return Mathf.Round(transform.position.x / (MeshPlane.tileSize.x * Chunk.width));
    }

    /// <summary>
    /// Our camera position in corrolation to the chunk position so if we are in chunk 0, 0 it returns 0, if we are in chunk 0, 1 we are in 1
    /// </summary>
    /// <returns></returns>
    public float ChunkZ()
    {
        return Mathf.Round(transform.position.z / (MeshPlane.tileSize.z * Chunk.length));
    }
}
