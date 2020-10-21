using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationBaker
{
    private List<NavMeshSurface> surfaces;

    public NavigationBaker()
    {
        surfaces = new List<NavMeshSurface>();
    }

    public void CreateNavMesh()
    {
        for(int i = 0; i < surfaces.Count; ++i)
        {
            surfaces[i].BuildNavMesh();
            //NavMeshBuilder.BuildNavMeshData(surfaces[i].GetBuildSettings, surfaces[i].navMeshData, new Bounds(transform.position, new Vector3(280, 10, 280)), transform.position, transform.rotation);
        }
    }

    public void AddSurface(NavMeshSurface newMesh)
    {
        surfaces.Add(newMesh);
    }

    public void BakeSurface(NavMeshSurface navMesh)
    {
        navMesh.BuildNavMesh();
    }


}
