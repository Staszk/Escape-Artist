using UnityEngine;
using System;

public class RoomTrigger : MonoBehaviour
{
    public static event Action<Room> ActionPlayerEnteredRegion = delegate { };

    private Room parent;

    public void SetupTrigger(Room parent)
    {
        this.parent = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActionPlayerEnteredRegion(parent);
        }
    }
}
