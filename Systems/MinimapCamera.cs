using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    private Vector3 offset = new Vector3(0.0f, 40f, 0.0f);

    public void MoveLocation(Room room)
    {
        transform.position = room.transform.position + offset;
    }
}
