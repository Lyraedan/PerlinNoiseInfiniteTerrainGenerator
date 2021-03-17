using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Author Zel
/// </summary>
public class Water : MonoBehaviour
{

    public float waterLevel = 11.5f;
    public float speed = 2.5f;
    public float strength = 1;
    [Range(0, 255)] public float transparency = 90;
    public Color color = new Color(0, 80, 180);
    [HideInInspector] public List<MeshPlane> mesh = new List<MeshPlane>();
    private Chunk chunk;

    private bool heightCalculated = false, meshBuilt = false, combinedMesh = false;

    [HideInInspector] public MeshFilter filter;
    private new MeshRenderer renderer;

    /// <summary>
    /// Set the chunk this water mesh is assigned to
    /// </summary>
    /// <param name="chunk"></param>
    public void SetChunk(Chunk chunk) => this.chunk = chunk;

    /// <summary>
    /// Generate the water for the assigned chunk
    /// </summary>
    public void GenerateWater()
    {
        GenerateFlatMesh();
        StartCoroutine(HandleWater());
    }

    /// <summary>
    /// Do we want to render our water?
    /// </summary>
    /// <param name="state"></param>
    public void RenderWater(bool state)
    {
        if (renderer)
            renderer.enabled = state;
    }

    /// <summary>
    /// Generate the water mesh in the designated chunk
    /// </summary>
    void GenerateFlatMesh()
    {
        for (int x = 0; x < Chunk.width; x++)
        {
            for (int z = 0; z < Chunk.length; z++)
            {
                GameObject obj = Instantiate(chunk.meshPlanePrefab, transform);
                MeshPlane plane = obj.GetComponent<MeshPlane>();
                plane.SetupMesh();
                plane.SetPosition(chunk.offsetX + (x * MeshPlane.tileSize.x), chunk.offsetZ + (z * MeshPlane.tileSize.z), waterLevel);
                plane.Build();
                plane.BuildMesh();
                string key = $"Tile_{x}_{z}";
                obj.name = key;
                mesh.Add(plane);
            }
        }
    }

    /// <summary>
    /// Handle our water mesh, calculate the height, rebuild the mesh, combine our mesh objects, cleanup redundancies
    /// </summary>
    /// <returns></returns>
    IEnumerator HandleWater()
    {
        ThreadPool.QueueUserWorkItem(CalculateHeight);
        yield return new WaitUntil(() => heightCalculated);
        for (int i = 0; i < mesh.Count; i++)
        {
            MeshPlane plane = mesh[i];
            plane.BuildMesh();

            if (i <= mesh.Count - 1)
                meshBuilt = true;
        }
        yield return new WaitUntil(() => meshBuilt);
        CombineMeshes();
        yield return new WaitUntil(() => combinedMesh);
        Cleanup();
    }

    /// <summary>
    /// Calculate the height of our water and remove any vertices that are below the terrain level
    /// </summary>
    void CalculateHeight(object state)
    {
        // Todo optimize? - memory leak?
        foreach (MeshPlane terrainMesh in chunk.mesh)
        {
            foreach (MeshPlane plane in mesh)
            {
                for (int i = 0; i < plane.vertices.Length; i++)
                {
                    for (int j = 0; j < terrainMesh.vertices.Length; j++)
                    {
                        //Debug.Log("Comparing plane y " + plane.vertices[i].y + " to Terrain vertice " + terrainMesh.vertices[j].y);
                        if (plane.vertices[i].y < terrainMesh.vertices[j].y)
                        {
                            //Remove here
                            //plane.vertices.RemoveAt(i);
                            // Update triangles
                            //0, 2, 1, 2, 3, 1


                            //I need to sit down and carefully thing about this
                            int[] newTris = new int[6] {
                                0, 2, 1, 2, 3, 1
                            };
                            
                            //plane.indices = newTris;
                        }
                    }
                }
            }
        }
        heightCalculated = true;
    }

    /// <summary>
    /// Assign our built water mesh the colour we desire
    /// </summary>
    void ColourMesh()
    {
        color.a = transparency;
        renderer.sharedMaterial.SetColor("_Color", color);
    }

    /// <summary>
    /// Combine all the mesh object meshes into one master mesh and apply that to the chunk gameobject
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

        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combine);

        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Custom/Water"));
        renderer.sharedMaterial.SetFloat("_Speed", speed);
        renderer.sharedMaterial.SetFloat("_Strength", strength);
        color.a = transparency;
        renderer.sharedMaterial.SetColor("_Color", color);

        transform.gameObject.SetActive(true);
        combinedMesh = true;
    }

    /// <summary>
    /// Clean up the waters mesh game objects as we no longer need them
    /// </summary>
    void Cleanup()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        mesh.Clear();
    }
}
