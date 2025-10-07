using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    [Header("Terrain")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private int resolution = 256;
    [SerializeField] private float scale = 20f;
    [SerializeField] private float heightMultiplier = 0.1f;
    [SerializeField] private bool randomizeOffset = true; 
    [SerializeField] private float offsetX = 100f;
    [SerializeField] private float offsetY = 100f;
    [SerializeField, Range(0f, 1f)] private float noiseThreshold;

    [Header("Textures by height")]
    [SerializeField] private TerrainLayer[] terrainLayers;

    [Header("Vegetation")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private int treeCount;
    [SerializeField] private float maxSlope = 30f;       
    [SerializeField, Range(0f, 1f)] private float vegetationSpawnGrassThreshold;       

    void Start()
    {
        if (randomizeOffset)
        {
            offsetX = Random.Range(0f, 1000f);
            offsetY = Random.Range(0f, 1000f);
        }

        GenerateTerrain();
        ApplyTextures();
        SpawnVegetation();
    }

    void GenerateTerrain()
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.heightmapResolution = resolution;

        float[,] heights = new float[resolution, resolution];

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float xCoord = (float)x / resolution * scale + offsetX;
                float yCoord = (float)y / resolution * scale + offsetY;

                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                heights[x, y] = sample * heightMultiplier;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
    
    void ApplyTextures()
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.terrainLayers = terrainLayers;

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        int numLayers = terrainLayers.Length;

        float[,,] splatmapData = new float[width, height, numLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normX = (float)x / width;
                float normY = (float)y / height;

                float terrainHeight = terrainData.GetHeight(
                    Mathf.RoundToInt(normY * terrainData.heightmapResolution),
                    Mathf.RoundToInt(normX * terrainData.heightmapResolution)
                ) / terrainData.size.y;

                float[] weights = new float[numLayers];
                
                float sandToGrass = Mathf.InverseLerp(0.0f, 0.15f, terrainHeight);
                float grassToDirt = Mathf.InverseLerp(0.3f, 0.5f, terrainHeight);
                float dirtToSnow  = Mathf.InverseLerp(0.55f, 0.75f, terrainHeight);

                if (numLayers > 0) weights[0] = 1.0f - sandToGrass;
                if (numLayers > 1) weights[1] = sandToGrass * (1.0f - grassToDirt);
                if (numLayers > 2) weights[2] = grassToDirt * (1.0f - dirtToSnow);
                if (numLayers > 3) weights[3] = dirtToSnow;
                
                float total = 0;
                for (int i = 0; i < numLayers; i++) total += weights[i];
                for (int i = 0; i < numLayers; i++) weights[i] /= total;

                for (int i = 0; i < numLayers; i++)
                    splatmapData[x, y, i] = weights[i];
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void SpawnVegetation()
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        var treePositions = new System.Collections.Generic.List<Vector3>();
        int spawnedTree = 0;
        int targetLayer = 1;
        
        for (int x = 0; x < width && spawnedTree < treeCount; x++)
        {
            for (int y = 0; y < height && spawnedTree < treeCount; y++)
            {
                Vector3 worldPos = new Vector3(
                    x * terrainData.size.x / width,
                    0,
                    y * terrainData.size.z / height
                );
                worldPos.y = terrain.SampleHeight(worldPos) + terrain.GetPosition().y;

                if (IsGrass(worldPos))
                {
                    float noise = Mathf.PerlinNoise(x / scale, y / scale);
                    if (noise > noiseThreshold)
                    {
                        bool tooClose = false;
                        foreach (var pos in treePositions)
                        {
                            if (Vector3.Distance(pos, worldPos) < 5)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        if (!tooClose)
                        {
                            Instantiate(treePrefabs[Random.Range(0,treePrefabs.Length)], worldPos, Quaternion.identity);
                            treePositions.Add(worldPos);
                            spawnedTree++;
                        }
                    }
                }
            }
        }
    }
    
    public int GetMainTextureIndex(Vector3 worldPos)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = worldPos - terrain.transform.position;

        int mapX = Mathf.FloorToInt((terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt((terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight);

        float[,,] splatmap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float max = 0;
        int index = 0;

        for (int i = 0; i < splatmap.GetLength(2); i++)
        {
            if (splatmap[0, 0, i] > max)
            {
                max = splatmap[0, 0, i];
                index = i;
            }
        }

        return index; 
    }

    public bool IsGrass(Vector3 worldPos)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = worldPos - terrain.transform.position;

        int mapX = Mathf.FloorToInt((terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt((terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight);

        float[,,] splatmap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        int targetIndex = 1; //grass
        float targetTreshold = 0.95f; //target a depasser

        return splatmap[0, 0, targetIndex] > targetTreshold;
    }
}