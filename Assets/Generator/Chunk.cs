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

    private bool heightGenerated = false, builtTerrain = false, combinedMesh = false;

    private MeshFilter filter;
    private new MeshRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        GenerateTerrain();
    }

    public void SetOffset(float x, float z)
    {
        this.offsetX = x;
        this.offsetZ = z;
        transform.position = new Vector3(x, 0, z);
    }

    public void RenderChunk(bool state)
    {
        gameObject.name = World.instance.GetChunkKey(offsetX, offsetZ) + $" <{state}>";
        if (renderer)
            renderer.enabled = state;
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
        //while (!heightGenerated) yield return null;
        yield return new WaitUntil(() => heightGenerated);
        for (int i = 0; i < mesh.Count; i++)
        {
            MeshPlane plane = mesh[i];
            plane.ColourMesh();
            plane.BuildMesh();

            if (i >= mesh.Count - 1)
                builtTerrain = true;
        }
        yield return new WaitUntil(() => builtTerrain);
        CombineMeshes();
        yield return new WaitUntil(() => combinedMesh);
        GenerateColliders();
        Cleanup();
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

    void CombineMeshes()
    {
        List<MeshFilter> filters = new List<MeshFilter>();
        foreach (MeshPlane plane in mesh)
        {
            filters.Add(plane.filter);
        }
        Debug.Log("Combining " + filters.Count + " filters");
        CombineInstance[] combine = new CombineInstance[filters.Count];
        int i = 0;
        while (i < filters.Count)
        {
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.worldToLocalMatrix;
            //filters[i].gameObject.SetActive(false);
            i++;
        }
        filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combine);

        renderer = gameObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Standard"));

        transform.gameObject.SetActive(true);
        combinedMesh = true;
    }

    void GenerateColliders()
    {
        MeshCollider collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = filter.mesh;
    }

    void Cleanup()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        mesh.Clear();
    }
}
