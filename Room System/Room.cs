using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public enum RoomType
{
    Edge,
    Gala,
    Civilian,
    Secure
}

[RequireComponent(typeof(NavMeshSurface))]
public class Room : MonoBehaviour
{
    protected RoomType roomType;

    public int X { get; private set; }
    public int Z { get; private set; }

    public List<Transform> wallSpawnLocations;

    public List<ObjectSpawnNode> spawnNodes;

    public RoomTrigger[] roomTriggers;
    protected string roomName;
    protected List<AudienceInteractable> audienceInteractables;

    // Minimap
    private GameObject minimapObject;
    private Material discoveredMat;
    private Material hiddenMat;
    private bool visited = false;

    public GameObject doorObject;

    public virtual void Setup(int x, int z, string name, RoomType typeOfRoom, bool[] doors, GameObject[] wallObjects)
    {
        roomName = name;
        roomType = typeOfRoom;

        X = x;
        Z = z;

        foreach (RoomTrigger trigger in roomTriggers)
        {
            trigger.SetupTrigger(this);
        }

        audienceInteractables = new List<AudienceInteractable>();

        // Spawn walls after creating interactables to add
        // doors to the list
        SpawnWalls(doors, wallObjects);

        foreach (ObjectSpawnNode node in spawnNodes)
        {
            AudienceInteractable audInt = node.SpawnObject();

            if (audInt != null)
            {
                InitAudienceInteractable(audInt);
            }
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Minimap"))
            {
                minimapObject = child.gameObject;
                discoveredMat = minimapObject.GetComponent<MeshRenderer>().material;
                break;
            }
        }

        spawnNodes.Clear();
    }

    public void VisitRoom()
    {
        if (!visited)
        {
            minimapObject.GetComponent<MeshRenderer>().material = discoveredMat;
            visited = true;
        }
    }

    public void GiveMaterial(Material mat)
    {
        hiddenMat = mat;

        if (minimapObject == null)
        {
            Debug.LogWarning("No minimap object");
            return;
        }

        if (!(this is PlayerStartRoom))
        {
            minimapObject.GetComponent<MeshRenderer>().material = hiddenMat;
        }
    }

    private void SpawnWalls(bool[] doors, GameObject[] wallObjects)
    {
        for (int i = 0; i < doors.Length; i++)
        {
            int type = doors[i] ? 0 : 1;
            GameObject wall = Instantiate(wallObjects[type], wallSpawnLocations[i].position, wallSpawnLocations[i].rotation);

            // If this wall segment has a space for a door
            // and this room "owns" that wall segment
            if (doors[i] && i < 2)
            {
                // Spawn door and initialize it
                Transform doorOpening = wall.transform.GetChild(0);

                DoorInteractable thisDoor = Instantiate(doorObject, doorOpening).GetComponent<DoorInteractable>();

                thisDoor.transform.SetParent(transform);

                InitAudienceInteractable(thisDoor);
            }
        }
    }

    protected void InitAudienceInteractable(AudienceInteractable audInt)
    {
        InteractableState state = Random.value > 0.5 ? InteractableState.negative : InteractableState.positive;
        audInt.InitializeInteractable(roomName, state, this);
        audienceInteractables.Add(audInt);
    }

    public List<AudienceInteractable> GetInteractables()
    {
        return audienceInteractables;
    }

    public RoomType GetRoomType()
    {
        return roomType;
    }

    public static RoomType GetRoomTypeFromGridSpace(GridSpace space)
    {
        switch (space)
        {
            case GridSpace.secure:
                return RoomType.Secure;
            case GridSpace.gala:
                return RoomType.Gala;
            case GridSpace.civilian:
                return RoomType.Civilian;
            default:
                return RoomType.Secure;
        }
    }
}
