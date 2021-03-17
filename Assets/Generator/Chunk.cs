using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/*
 
    Author: Zel
     
 */
public class Chunk : MonoBehaviour
{

    public GameObject meshPlanePrefab;

    public static int width = 10, length = 10;

    [HideInInspector] public List<MeshPlane> mesh = new List<MeshPlane>();

    public float amplitude = 0.1f;
    [Header("Perlin Noise")]
    public int octaves = 1; // 4
    public double persistance = 2; // 3
    public double frequancy = 1;
    public double noiseAmplitude = 1;
    [Header("Noise clamping")]
    public bool clampingEnabled = false;
    public Vector2 clamp = new Vector2(0, 10);
    [Space(5)]
    public float offsetX = 0, offsetZ = 0;

    public float mod = 1f;

    // Chunk bools
    private bool heightGenerated = false, builtTerrain = false, combinedMesh = false;

    // Water bools
    private bool generatedWaterMesh = false;

    private MeshFilter filter;
    private new MeshRenderer renderer;
    private new MeshCollider collider;
    private Water water;

    /// <summary>
    /// What is the XZ offset for this chunk?
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void SetOffset(float x, float z)
    {
        this.offsetX = x;
        this.offsetZ = z;
        transform.position = new Vector3(x, 0, z);
    }

    /// <summary>
    /// Do we want the Renderer to be enabled for this chunk?
    /// </summary>
    /// <param name="state"></param>
    public void RenderChunk(bool state)
    {
        gameObject.name = World.instance.GetChunkKey(offsetX, offsetZ) + $" <{state}>";
        if (renderer)
            renderer.enabled = state;

        if (water)
            water.RenderWater(state);

        if (collider)
        {
            collider.enabled = state;
            // Uncertain
            //collider.sharedMesh = null;
        }
    }

    /// <summary>
    /// Generate our terrain
    /// </summary>
    public void GenerateTerrain()
    {
        GenerateFlatMesh();
        // Multithread
        StartCoroutine(GenerateHeightmap());
    }

    /// <summary>
    /// Generate a flat mesh for our chunk ready to be modified
    /// </summary>
    void GenerateFlatMesh()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject obj = Instantiate(meshPlanePrefab, transform);
                MeshPlane plane = obj.GetComponent<MeshPlane>();
                plane.SetupMesh();
                plane.SetPosition(offsetX + (x * MeshPlane.tileSize.x), offsetZ + (z * MeshPlane.tileSize.z));
                plane.Build();
                plane.BuildMesh();
                string key = $"Tile_{x}_{z}";
                obj.name = key;
                mesh.Add(plane);
            }
        }
    }

    /// <summary>
    /// Generate the heightmap, combine meshes, generate colliders and cleanup our terrain
    /// </summary>
    /// <returns></returns>
    IEnumerator GenerateHeightmap()
    {
        ThreadPool.QueueUserWorkItem(ApplyPerlin);
        //while (!heightGenerated) yield return null;
        yield return new WaitUntil(() => heightGenerated);
        for (int i = 0; i < mesh.Count; i++)
        {
            MeshPlane plane = mesh[i];
            //plane.ColourMesh();
            plane.BuildMesh();

            if (i >= mesh.Count - 1)
                builtTerrain = true;
        }
        yield return new WaitUntil(() => builtTerrain);
        CombineMeshes();
        yield return new WaitUntil(() => combinedMesh);
        GenerateColliders();
        GenerateWater();
        yield return new WaitUntil(() => generatedWaterMesh);
        Cleanup();
        ColorMesh();
    }

    void ColorMesh()
    {
        renderer.sharedMaterial.color = Color.green;
    }

    public void GenerateWater()
    {
        GameObject waterObject = new GameObject("Water");
        waterObject.transform.SetParent(transform);
        this.water = waterObject.AddComponent<Water>();
        water.SetChunk(this);
        water.filter = waterObject.AddComponent<MeshFilter>();
        water.GenerateWater();
        generatedWaterMesh = true;
    }

    /// <summary>
    /// Add the perlin noise heightmap to our terrain
    /// </summary>
    /// <param name="state"></param>
    void ApplyPerlin(object state)
    {
        foreach (MeshPlane plane in mesh)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    for (int i = 0; i < plane.vertices.Length; i++)
                    {
                        float perlinX = plane.vertices[i].x + (x * MeshPlane.tileSize.x) / width * amplitude;
                        float perlinZ = plane.vertices[i].z + (z * MeshPlane.tileSize.z) / length * amplitude;

                        #region OLD PERLIN
                        /*
                        float noise = (float)PerlinNoise.perlin(perlinX + World.instance.seed,
                                                                plane.vertices[i].y + World.instance.seed,
                                                                perlinZ + World.instance.seed);
                                                                */
                        /*
                        float noise = (float)PerlinNoise.OctavePerlin(perlinX + World.instance.seed,
                                      plane.vertices[i].y + World.instance.seed,
                                      perlinZ + World.instance.seed,
                                      octaves,
                                      persistance,
                                      frequancy,
                                      noiseAmplitude);
                                      */
                        #endregion

                        float noise = Mathf.PerlinNoise(perlinX + World.instance.offset.x, perlinZ + World.instance.offset.z) * 25f;

                        if (clampingEnabled)
                            plane.vertices[i].y = Mathf.Clamp(noise, clamp.x, clamp.y);
                        else
                            plane.vertices[i].y = noise;
                    }
                }
            }
            heightGenerated = true;
        }
    }

    /// <summary>
    /// Combnie all the mesh object meshes into one master mesh and apply that to the chunk gameobject
    /// </summary>
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

    /// <summary>
    /// Make our terrain solid by adding mesh colliders
    /// </summary>
    void GenerateColliders()
    {
        collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = filter.mesh;
    }

    /// <summary>
    /// Remove all the mesh game objects as they are no longer needed
    /// </summary>
    void Cleanup()
    {
        foreach (Transform child in transform)
        {
            if(!child.GetComponent<Water>())
                Destroy(child.gameObject);
        }
        mesh.Clear();
    }
}
