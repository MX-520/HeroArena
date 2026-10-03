using System.Collections;
using UnityEngine;
using NavMeshPlus.Components;

public class NavigationUpdater : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    public void RebuildNavMesh()
    {
        StartCoroutine(RebuildNextFrame());
    }

    private IEnumerator RebuildNextFrame()
    {
        yield return null;

        navMeshSurface.BuildNavMesh();
    }
}