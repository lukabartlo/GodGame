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

    [Header("Textures by height")]
    [SerializeField] private TerrainLayer[] terrainLayers;

    [Header("Vegetation")]
    [SerializeField] private GameObject[] treePrefabs; 
    [SerializeField] private int treeCount = 200;
    [SerializeField, Range(0f, 1f)] private float grassLayerThreshold;
    [SerializeField] private float maxSlope = 30f;       

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
        if (treePrefabs == null || treePrefabs.Length == 0) return;

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, width, height);

        int spawned = 0;
        int maxAttempts = treeCount * 5;

        for (int attempts = 0; attempts < maxAttempts && spawned < treeCount; attempts++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            float grassWeight = splatmapData[x, y, 1];

            if (grassWeight > grassLayerThreshold)
            {
                float normX = (float)x / (width - 1);
                float normY = (float)y / (height - 1);

                float steepness = terrainData.GetSteepness(normX, normY);
                if (steepness > maxSlope) continue;

                float worldX = normX * terrainData.size.x + Random.Range(-1f, 1f);
                float worldZ = normY * terrainData.size.z + Random.Range(-1f, 1f);
                float worldY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

                Vector3 pos = new Vector3(worldX, worldY, worldZ);

                GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                Instantiate(prefab, pos, Quaternion.identity, terrain.transform);

                spawned++;
            }
        }
    }
}