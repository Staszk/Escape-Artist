using UnityEngine;
using System.Collections;

public class EdgeRoom : Room
{
    public void Create(bool[] doors, GameObject[] wallObjects)
    {
        SpawnWalls(doors, wallObjects[1]);
    }

    private void SpawnWalls(bool[] walls, GameObject wall)
    {
        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i])
            {
                Instantiate(wall, wallSpawnLocations[i].position, wallSpawnLocations[i].rotation);
            }
        }
    }
}
