using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class RandomWallGenerator : MonoBehaviour
{
    [Header("Wall Size (World Units)")]
    public float width = 10f;
    public float height = 10f;
    public float depth = 1f;

    [Header("Wall Resolution (Vertices)")]
    public int widthSegments = 10;
    public int heightSegments = 10;

    [Header("Noise Settings")]
    public float noiseScale = 5f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Vector2 noiseOffset;

    [Header("Seed Settings")]
    public int seed = 0;
    public bool useRandomSeed = true;
    
    [Header("Prefab Placement")]
    public GameObject prefabToSpawn;
    public int numberOfPrefabs = 10;
    public int maxNumberOfPrefabs = 1000;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void GenerateWallMesh()
    {
        if (useRandomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        Random.InitState(seed);
        noiseOffset = new Vector2(Random.value * 1000f, Random.value * 1000f);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float dx = width / widthSegments;
        float dy = height / heightSegments;

        for (int y = 0; y <= heightSegments; y++)
        {
            for (int x = 0; x <= widthSegments; x++)
            {
                float posX = x * dx;
                float posY = y * dy;
                float z = GenerateNoise(x, y); // noise affects depth

                vertices.Add(new Vector3(posX, posY, z));
                uvs.Add(new Vector2((float)x / widthSegments, (float)y / heightSegments));
            }
        }

        for (int y = 0; y < heightSegments; y++)
        {
            for (int x = 0; x < widthSegments; x++)
            {
                int i = y * (widthSegments + 1) + x;

                triangles.Add(i);
                triangles.Add(i + widthSegments + 1);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + widthSegments + 1);
                triangles.Add(i + widthSegments + 2);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        GeneratePrefabs();
    }

    float GenerateNoise(int x, int y)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + noiseOffset.x) / noiseScale * frequency;
            float sampleY = (y + noiseOffset.y) / noiseScale * frequency;
            float perlin = Mathf.PerlinNoise(sampleX, sampleY);

            noiseHeight += perlin * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noiseHeight * depth;
    }

    public void GeneratePrefabs()
    {
        // Remove old children
        DestroyAllChildren();

        if (prefabToSpawn == null || numberOfPrefabs <= 0)
        {
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] verts = mesh.vertices;

        for (int i = 0; i < numberOfPrefabs; i++)
        {
            Vector3 localPos = verts[Random.Range(0, verts.Length)];
            Vector3 worldPos = transform.TransformPoint(localPos);

            GameObject obj = Instantiate(prefabToSpawn, worldPos, Quaternion.identity, transform);
        }
    }

    public void SetPrefabNumber(float input)
    {
        numberOfPrefabs = Mathf.RoundToInt(input * maxNumberOfPrefabs);
    }

    public void DestroyAllChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!Application.isPlaying)
            {
                Debug.Log("DestroyImmediate: " + child.name);
                DestroyImmediate(child.gameObject);
            }
            else
            {
                Debug.Log("Destroy: " + child.name);
                Destroy(child.gameObject, 1);
            }
        }
    }
}