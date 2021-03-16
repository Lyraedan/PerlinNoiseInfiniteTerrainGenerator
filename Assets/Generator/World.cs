using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        if (seed == 0)
            seed = Random.Range(-10000, 10000);

        for(int x = 0; x < 3; x++)
        {
            for(int z = 0; z < 3; z++)
            {
                Generate(x, z);
            }
        }
    }

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

    public bool ChunkExists(float x, float z)
    {
        return chunks.ContainsKey(GetChunkKey(x, z));
    }

    public Chunk GetChunkAt(float x, float z)
    {
        string key = GetChunkKey(x, z);
        if (!ChunkExists(x, z))
            Generate(x, z);
        return chunks[key];
    }

    public string GetChunkKey(float x, float z)
    {
        return $"Chunk_{x}_{z}";
    }
}
