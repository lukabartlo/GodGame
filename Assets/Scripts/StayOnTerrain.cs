using UnityEngine;

public class StayOnTerrain : MonoBehaviour
{
    void Update()
    {
        var _hasHitTerrain = 
            Physics.Raycast(transform.position + Vector3.up *1000, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain"));

        if (_hasHitTerrain)
        {
            transform.position = hit.point;
        }
    }
}
