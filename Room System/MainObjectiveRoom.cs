using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainObjectiveRoom : SecureRoom
{
    public override void Setup(int x, int z, string name, RoomType typeOfRoom, bool[] doors, GameObject[] wallObjects)
    {
        base.Setup(x, z, name, RoomType.Secure, doors, wallObjects);

        SpawnSideObjective();
    }
}
