using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerStartRoom : Room
{
    public static event Action<Transform> ActionPlayerTransformSpawned = delegate { };

    public GameObject playerObject;
    public Transform playerSpawnLocation;

    public override void Setup(int x, int z, string name, RoomType typeOfRoom, bool[] doors, GameObject[] wallObjects)
    {
        base.Setup(x, z, name, RoomType.Gala, doors, wallObjects);

        Transform player = Instantiate(playerObject, playerSpawnLocation.position, playerSpawnLocation.rotation).transform;

        ActionPlayerTransformSpawned(player);
    }
}
