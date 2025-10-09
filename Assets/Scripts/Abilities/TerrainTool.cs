using UnityEngine;

public class TerrainEditor : Abilities
{
    public Terrain terrain;                // the Terrain component
    public float brushSize = 5f;           // in world units
    public float strength = 0.5f;          // how fast raise/lower (units per second)
    public bool lower = false;             // flag: raise or lower

    TerrainData terrainData;
    int heightmapWidth;
    int heightmapHeight;

    void Start()
    {
        terrainData = terrain.terrainData;
        heightmapWidth = terrainData.heightmapResolution;
        heightmapHeight = terrainData.heightmapResolution;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))  // e.g. left-click to terraform
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ModifyTerrain(hit.point);
            }
        }
    }

    void ModifyTerrain(Vector3 worldPoint)
    {
        // Convert world point to terrain local coordinates (0..1)
        Vector3 terrainPos = worldPoint - terrain.transform.position;
        Vector3 normalized = new Vector3(
            terrainPos.x / terrainData.size.x,
            0,
            terrainPos.z / terrainData.size.z
        );

        int cx = (int)(normalized.x * (heightmapWidth - 1));
        int cz = (int)(normalized.z * (heightmapHeight - 1));

        // Determine how many heightmap samples your brush covers
        float worldToHeight = (heightmapWidth - 1) / terrainData.size.x;
        int radius = Mathf.RoundToInt(brushSize * worldToHeight);

        int x0 = Mathf.Clamp(cx - radius, 0, heightmapWidth - 1);
        int x1 = Mathf.Clamp(cx + radius, 0, heightmapWidth - 1);
        int z0 = Mathf.Clamp(cz - radius, 0, heightmapHeight - 1);
        int z1 = Mathf.Clamp(cz + radius, 0, heightmapHeight - 1);
        int w = x1 - x0 + 1;
        int h = z1 - z0 + 1;

        // Get existing heights
        float[,] heights = terrainData.GetHeights(x0, z0, w, h);

        // Modify them
        for (int dz = 0; dz < h; dz++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                float old = heights[dz, dx];
                // you can apply a falloff, brush shape, etc. Here simple:
                float delta = strength * Time.deltaTime;
                heights[dz, dx] = lower
                    ? old - delta
                    : old + delta;
                heights[dz, dx] = Mathf.Clamp01(heights[dz, dx]);
            }
        }

        // Write back
        terrainData.SetHeights(x0, z0, heights);
        // If using SetHeightsDelayLOD variant, you would call SyncHeightmap after finishing editing.
    }
}