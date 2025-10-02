using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private int resolution = 256;
    [SerializeField] private float scale = 20f;
    [SerializeField] private float heightMultiplier = 0.1f;
    [SerializeField] private float offsetX = 100f;
    [SerializeField] private float offsetY = 100f;
    
    [Header("Textures by Height")]
    [SerializeField] private TerrainLayer[] terrainLayers; 

    void Start()
    {
        offsetX = Random.Range(0f, 1000f);
        offsetY = Random.Range(0f, 1000f);
        GenerateTerrain();
        ApplyTextures();
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
        terrainData.terrainLayers = terrainLayers; // Assign textures

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        int numLayers = terrainLayers.Length;

        float[,,] splatmapData = new float[width, height, numLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normX = (float)x / (float)width;
                float normY = (float)y / (float)height;
                float terrainHeight = terrainData.GetHeight(
                    Mathf.RoundToInt(normY * terrainData.heightmapResolution),
                    Mathf.RoundToInt(normX * terrainData.heightmapResolution)
                ) / terrainData.size.y;

                float[] weights = new float[numLayers];

                float sandToGrass = Mathf.InverseLerp(0.0f, 0.15f, terrainHeight);
                float grassToDirt = Mathf.InverseLerp(0.3f, 0.5f, terrainHeight);
                float dirtToSnow  = Mathf.InverseLerp(0.55f, 0.75f, terrainHeight);

                weights[0] = 1.0f - sandToGrass;
                weights[1] = sandToGrass * (1.0f - grassToDirt);
                weights[2] = grassToDirt * (1.0f - dirtToSnow);
                weights[3] = dirtToSnow;

                // Normalize
                float total = 0;
                for (int i = 0; i < numLayers; i++) total += weights[i];
                for (int i = 0; i < numLayers; i++) weights[i] /= total;

                for (int i = 0; i < numLayers; i++)
                    splatmapData[x, y, i] = weights[i];
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
}