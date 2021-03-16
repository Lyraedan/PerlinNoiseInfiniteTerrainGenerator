using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private Vector3 lastPos;

    void Update()
    {
        if (Vector3.Distance(lastPos, transform.position) < 1f) return;
        lastPos = transform.position;
        GenerateChunksAroundMe();
        MakeChunksAroundMeVisible();
        HideOutOfViewChunks();
    }

    public void MakeChunksAroundMeVisible()
    {
        float x = ChunkX();
        float z = ChunkZ();

        // Top row
        World.instance.GetChunkAt(x - 1, z - 1).RenderChunk(true);
        World.instance.GetChunkAt(x, z - 1).RenderChunk(true);
        World.instance.GetChunkAt(x + 1, z - 1).RenderChunk(true);

        // Middle row
        World.instance.GetChunkAt(x - 1, z).RenderChunk(true);
        World.instance.GetChunkAt(x, z).RenderChunk(true);
        World.instance.GetChunkAt(x + 1, z).RenderChunk(true);

        // Bottom row
        World.instance.GetChunkAt(x - 1, z + 1).RenderChunk(true);
        World.instance.GetChunkAt(x, z + 1).RenderChunk(true);
        World.instance.GetChunkAt(x + 1, z + 1).RenderChunk(true);

    }

    /// <summary>
    /// Memory leak
    /// </summary>
    public void GenerateChunksAroundMe()
    {
        float x = ChunkX();
        float z = ChunkZ();

        // Top row
        World.instance.Generate(x - 1, z - 1);
        World.instance.Generate(x, z - 1);
        World.instance.Generate(x + 1, z - 1);

        // Middle row
        World.instance.Generate(x - 1, z);
        World.instance.Generate(x, z);
        World.instance.Generate(x + 1, z);

        // Bottom row
        World.instance.Generate(x - 1, z + 1);
        World.instance.Generate(x, z + 1);
        World.instance.Generate(x + 1, z + 1);

    }

    public void HideOutOfViewChunks()
    {
        float x = ChunkX();
        float z = ChunkZ();

        // Left
        World.instance.GetChunkAt(x - 2, z - 1).RenderChunk(false);
        World.instance.GetChunkAt(x - 2, z).RenderChunk(false);
        World.instance.GetChunkAt(x - 2, z + 1).RenderChunk(false);

        // Right
        World.instance.GetChunkAt(x + 2, z - 1).RenderChunk(false);
        World.instance.GetChunkAt(x + 2, z).RenderChunk(false);
        World.instance.GetChunkAt(x + 2, z + 1).RenderChunk(false);

        // Bottom
        World.instance.GetChunkAt(x - 1, z - 2).RenderChunk(false);
        World.instance.GetChunkAt(x, z - 2).RenderChunk(false);
        World.instance.GetChunkAt(x + 1, z - 2).RenderChunk(false);

        // Top
        World.instance.GetChunkAt(x - 1, z + 2).RenderChunk(false);
        World.instance.GetChunkAt(x, z + 2).RenderChunk(false);
        World.instance.GetChunkAt(x + 1, z + 2).RenderChunk(false);

        // Corners
        World.instance.GetChunkAt(x - 2, z + 2).RenderChunk(false);
        World.instance.GetChunkAt(x + 2, z + 2).RenderChunk(false);
        World.instance.GetChunkAt(x - 2, z - 2).RenderChunk(false);
        World.instance.GetChunkAt(x + 2, z - 2).RenderChunk(false);

    }

    public float ChunkX()
    {
        return Mathf.Round(transform.position.x / (MeshPlane.tileSize.x * Chunk.width));
    }

    public float ChunkZ()
    {
        return Mathf.Round(transform.position.z / (MeshPlane.tileSize.z * Chunk.length));
    }
}
