using Unity.AI.Navigation;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private NavMeshSurface _surface;
    void Start()
    {
        _surface.BuildNavMesh();
    }
}
