using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    [SerializeField] private Terrain terrain; // Assigne ton terrain dans l’inspecteur
    [SerializeField] private int resolution = 256; // Résolution de la heightmap
    [SerializeField] private float scale = 20f; // Zoom du bruit
    [SerializeField] private float heightMultiplier = 0.1f; // Hauteur max des montagnes
    [SerializeField] private float offsetX = 100f; // Décalage X
    [SerializeField] private float offsetY = 100f; // Décalage Y

    void Start()
    {
        offsetX = Random.Range(0f, 1000f);
        offsetY = Random.Range(0f, 1000f);
        GenerateTerrain();
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
}