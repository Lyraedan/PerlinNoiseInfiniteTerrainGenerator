using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk : MonoBehaviour
{

    public GameObject meshPlanePrefab;

    public static int width = 10, length = 10;

    private List<MeshPlane> mesh = new List<MeshPlane>();

    public float amplitude = 0.1f;
    [Header("Perlin Noise")]
    public int octaves = 1; // 4
    public double persistance = 2; // 3
    public double frequancy = 1;
    public double noiseAmplitude = 1;
    [Space(5)]
    public float offsetX = 0, offsetZ = 0;

    public float mod = 1f;

    private bool heightGenerated = false;

    // Start is called before the first frame update
    void Start()
    {
        GenerateTerrain();
    }

    public void SetOffset(float x, float z)
    {
        this.offsetX = x;
        this.offsetZ = z;
    }

    public void RenderChunk(bool state)
    {
        gameObject.name = World.instance.GetChunkKey(offsetX, offsetZ) + $" <{state}>";
        foreach(MeshPlane plane in mesh)
        {
            plane.gameObject.SetActive(state);
        }
    }

    public void GenerateTerrain()
    {
        GenerateFlatMesh();
        // Multithread
        StartCoroutine(GenerateHeightmap());
    }

    void GenerateFlatMesh()
    {
        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < length; z++)
            {
                GameObject obj = Instantiate(meshPlanePrefab, transform);
                MeshPlane plane = obj.GetComponent<MeshPlane>();
                plane.SetupMesh();
                plane.SetPosition(offsetX + (x * MeshPlane.tileSize.x), offsetZ + (z * MeshPlane.tileSize.z));
                plane.Build();
                plane.BuildMesh();
                string key = $"Tile_{plane.x / width}_{plane.z / length}";
                obj.name = key;
                mesh.Add(plane);
            }
        }
    }

    IEnumerator GenerateHeightmap()
    {
        ThreadPool.QueueUserWorkItem(ApplyPerlin);
        while (!heightGenerated) yield return null;
        foreach(MeshPlane plane in mesh)
        {
            plane.ColourMesh();
            plane.BuildMesh();
        }
    }

    void ApplyPerlin(object state)
    {
        foreach(MeshPlane plane in mesh)
        {
            for(int x = 0; x < width; x++)
            {
                for(int z = 0; z < length; z++)
                {
                    for (int i = 0; i < plane.vertices.Length; i++)
                    {
                        float perlinX = plane.vertices[i].x + (x * MeshPlane.tileSize.x) / width * amplitude;
                        float perlinZ = plane.vertices[i].z + (z * MeshPlane.tileSize.z) / length * amplitude;

                        /*float noise = (float)PerlinNoise.perlin(perlinX + World.instance.seed,
                                                                plane.vertices[i].y + World.instance.seed,
                                                                perlinZ + World.instance.seed);*/
                        float noise = (float)PerlinNoise.OctavePerlin(perlinX + World.instance.seed,
                                                                      plane.vertices[i].y + World.instance.seed,
                                                                      perlinZ + World.instance.seed,
                                                                      octaves,
                                                                      persistance,
                                                                      frequancy,
                                                                      noiseAmplitude);
                        plane.vertices[i].y = noise;
                    }
                }
            }
            //plane.BuildMesh();
            heightGenerated = true;
        }
    }
}
