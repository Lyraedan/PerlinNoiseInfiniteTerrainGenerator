using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    Author: Zel
 
*/
public class World : MonoBehaviour
{

    public static World instance;
    public GameObject chunkPrefab;
    public Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();

    public float seed = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        if (seed == 0)
            seed = Random.Range(-10000, 10000);
    }

    /// <summary>
    /// Loop through any active chunks and if they are out of range disable them
    /// </summary>
    public void HideChunks()
    {
        foreach(Chunk chunk in chunks.Values)
        {
            if(chunk.gameObject.activeSelf)
                chunk.RenderChunk(false);
        }
    }

    /// <summary>
    /// Generate a chunk at the given coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void Generate(float x, float z)
    {
        string key = GetChunkKey(x, z);
        if (chunks.ContainsKey(key)) return;

        GameObject obj = Instantiate(chunkPrefab, transform);
        obj.name = key;
        Chunk chunk = obj.GetComponent<Chunk>();
        chunk.SetOffset(x * MeshPlane.tileSize.x * Chunk.width,
                        z * MeshPlane.tileSize.z * Chunk.length);
        chunk.GenerateTerrain();
        
        chunks.Add(key, chunk);
    }

    /// <summary>
    /// Generate an empy chunk (null) at the given coordinates - Useful for testing and debugging memory leaks
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void GenerateEmpty(float x, float z)
    {
        string key = GetChunkKey(x, z);
        if (chunks.ContainsKey(key)) return;
        chunks.Add(key, null);
    }

    /// <summary>
    /// Does a chunk exist at the given x and z value
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public bool ChunkExists(float x, float z)
    {
        return chunks.ContainsKey(GetChunkKey(x, z));
    }

    /// <summary>
    /// Get the instance of the chunk at a given x, z coordinate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>. If none exist. Generate it
    public Chunk GetChunkAt(float x, float z)
    {
        string key = GetChunkKey(x, z);
        if (!ChunkExists(x, z))
            Generate(x, z);
        return chunks[key];
    }

    /// <summary>
    /// Get the key that is used in the dictionary for the chunks
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public string GetChunkKey(float x, float z)
    {
        return $"Chunk_{x}_{z}";
    }
}
