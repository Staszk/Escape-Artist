using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoomManager : MonoBehaviour
{
    #region Singleton
    public static RoomManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    private NavigationBaker navMeshBaker;

    private Room[,] roomLayout;

    public List<GameObject> possibleSecureRooms;
    public List<GameObject> possibleCivilianRooms;
    public List<GameObject> possibleGalaRooms;
    public GameObject edgeRoom;
    public GameObject objectiveRoom;

    public GameObject enemyPrefab;
    public GameObject[] wallTypes;

    private int col, row;

    private Room startingRoom = null;
    private Room currentRoom;

    private List<SecureRoom> spawnedSecureRooms;

    public GameObject alertQuad;

    private string alphabet = "ABCDEFGHIJ";

    [Header("Minimap")]
    public Material hiddenMat;

    public void SpawnLayout(GridSpace[,] gridSpaces, int numberOfSecureRooms)
    {
        col = gridSpaces.GetLength(0);
        row = gridSpaces.GetLength(1);

        roomLayout = new Room[col, row];

        // Order: Spawn Colliding Objects; Create NavMesh; Spawn Trigger Objects; Spawn Guards
        navMeshBaker = new NavigationBaker();
        SpawnRooms(gridSpaces, col, row);
        SpawnSideObjectives();
        navMeshBaker.CreateNavMesh();
        SpawnGuards(numberOfSecureRooms);
    }

    private void SpawnRooms(GridSpace[,] gridSpaces, int col, int row)
    {
        spawnedSecureRooms = new List<SecureRoom>();

        for(int i = 0; i < col; ++i)
        {
            for (int j = 0; j < row; j++)
            {
                GameObject room = null;
                Vector3 pos = new Vector3(28 * i - (2 * 28), 0, 28 * j - (2 * 28));
                int randomIndex;

                GridSpace thisSpot = gridSpaces[i,j];

                if (thisSpot == GridSpace.objective)
                {
                    room = Instantiate(objectiveRoom, pos, Quaternion.identity);
                }
                else if (thisSpot == GridSpace.secure)
                {
                    //Grabs Random Room from the List based on index
                    randomIndex = Random.Range(0, possibleSecureRooms.Count);

                    room = Instantiate(possibleSecureRooms[randomIndex], pos, Quaternion.identity);
                }
                else if (thisSpot == GridSpace.civilian)
                {
                    //Grabs Random Room from the List based on index
                    randomIndex = Random.Range(0, possibleCivilianRooms.Count);

                    room = Instantiate(possibleCivilianRooms[randomIndex], pos, Quaternion.identity);
                }
                else if (thisSpot == GridSpace.gala)
                {
                    //Grabs Random Room from the List based on index
                    randomIndex = Random.Range(1, possibleGalaRooms.Count);

                    if (startingRoom == null)
                    {
                        room = Instantiate(possibleGalaRooms[0], pos, Quaternion.identity);
                        startingRoom = room.GetComponent<Room>();
                        currentRoom = startingRoom;
                    }
                    else
                    {
                        room = Instantiate(possibleGalaRooms[randomIndex], pos, Quaternion.identity);
                    }
                }
                else if (thisSpot == GridSpace.edge)
                {
                    room = Instantiate(edgeRoom, pos, Quaternion.identity);

                    room.GetComponent<EdgeRoom>().Create(CreateEdge(gridSpaces, i, j), wallTypes);

                    room.transform.SetParent(transform);

                    continue;
                }

                if (room != null)
                {
                    room.transform.SetParent(transform);
                    roomLayout[i, j] = room.GetComponent<Room>();

                    if (thisSpot == GridSpace.secure || thisSpot == GridSpace.objective)
                    {
                        SecureRoom scr = room.GetComponent<SecureRoom>();

                        spawnedSecureRooms.Add(scr);

                        scr.GiveQuad(alertQuad);
                    }

                    string alphanumeric = alphabet.Substring(i, 1) + (roomLayout.GetLength(1) - j).ToString();

                    bool[] doors = CheckWalls(gridSpaces, i, j);

                    roomLayout[i, j].Setup(i, j, alphanumeric, Room.GetRoomTypeFromGridSpace(thisSpot), doors, wallTypes);
                    roomLayout[i, j].GiveMaterial(hiddenMat);

                    if (thisSpot != GridSpace.edge)
                    {
                        navMeshBaker.AddSurface(room.GetComponent<NavMeshSurface>());
                    }
                }
            }
        }
    }

    private bool[] CreateEdge(GridSpace[,] gridSpaces, int i, int j)
    {
        bool[] wallCheck = { false, false, false, false };

        // Check Top Wall
        if (j < gridSpaces.GetLength(1) - 1)
        {
            if (gridSpaces[i, j + 1] != GridSpace.edge && gridSpaces[i, j + 1] != GridSpace.empty)
            {
                wallCheck[0] = true;
            }
        }

        // Check Right Wall
        if (i < gridSpaces.GetLength(0) - 1)
        {
            if (gridSpaces[i + 1, j] != GridSpace.edge && gridSpaces[i + 1, j] != GridSpace.empty)
            {
                wallCheck[1] = true;
            }
        }

        // Check Bottom Wall
        if (j > 0)
        {
            if (gridSpaces[i, j - 1] != GridSpace.edge && gridSpaces[i, j - 1] != GridSpace.empty)
            {
                wallCheck[2] = true;
            }
        }

        // Check Left Wall
        if (i > 0)
        {
            if (gridSpaces[i - 1, j] != GridSpace.edge && gridSpaces[i - 1, j] != GridSpace.empty)
            {
                wallCheck[3] = true;
            }
        }

        return wallCheck;
    }

    private bool[] CheckWalls(GridSpace[,] gridSpaces, int i, int j)
    {
        bool[] wallCheck = { false, false, false, false };

        // Check Top Wall
        if (j < gridSpaces.GetLength(1) - 1)
        {
            if (gridSpaces[i, j + 1] != GridSpace.empty && gridSpaces[i, j + 1] != GridSpace.edge)
            {
                wallCheck[0] = true;
            }
        }

        // Check Right Wall
        if (i < gridSpaces.GetLength(0) - 1)
        {
            if (gridSpaces[i + 1, j] != GridSpace.empty && gridSpaces[i + 1, j] != GridSpace.edge)
            {
                wallCheck[1] = true;
            }
        }

        // Check Bottom Wall
        if (j > 0)
        {
            if (gridSpaces[i, j - 1] != GridSpace.empty && gridSpaces[i, j - 1] != GridSpace.edge)
            {
                wallCheck[2] = true;
            }
        }

        // Check Left Wall
        if (i > 0)
        {
            if (gridSpaces[i - 1, j] != GridSpace.empty && gridSpaces[i - 1, j] != GridSpace.edge)
            {
                wallCheck[3] = true;
            }
        }

        return wallCheck;
    }

    private void SpawnSideObjectives()
    {
        foreach (SecureRoom sr in spawnedSecureRooms)
        {
            sr.SpawnSideObjective();
        }
    }

    private void SpawnGuards(int numSecureRooms)
    {
        int startingGuardNum = numSecureRooms + 4;

        int currentNum = 0;

        foreach (SecureRoom sr in spawnedSecureRooms)
        {
            // Spawn guard in this room
            sr.SpawnGuard(enemyPrefab);

            currentNum++;
        }

        while (currentNum < startingGuardNum)
        {
            int randomRoom = Random.Range(0, spawnedSecureRooms.Count);

            // Spawn guard in this room
            spawnedSecureRooms[randomRoom].SpawnGuard(enemyPrefab);

            currentNum++;
        }
    }

    public Room GetStartingRoom()
    {
        return startingRoom;
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    public void ChangeRoom(Room newRoom)
    {
        currentRoom = newRoom;
    }

    public AudienceInteractable GetInteractableOfType(int ID, InteractableState state)
    {
        List<AudienceInteractable> interactablesToCheck;
        List<AudienceInteractable> interactableOptions = new List<AudienceInteractable>();

        // Start with current room
        Room roomToCheck = currentRoom;

        // Get Interactables from room
        interactablesToCheck = roomToCheck.GetInteractables();

        foreach (AudienceInteractable audInt in interactablesToCheck)
        {
            if (audInt.GetID() == ID && audInt.state == state)
            {
                interactableOptions.Add(audInt);
            }
        }

        if (interactableOptions.Count > 0)
        {
            // Pick one
            int randomIndex = Random.Range(0, interactableOptions.Count);

            return interactableOptions[randomIndex];
        }
        // We need to check all nearby rooms
        else
        {
            for (int i = currentRoom.X - 1; i < currentRoom.X + 2; i++)
            {
                for (int j = currentRoom.Z - 1; j < currentRoom.Z + 2; j++)
                {
                    // Make sure this spot is not out of bounds and not current room
                    if ((i > 0 && i < col) && (j > 0 && j < row) && roomLayout[i,j] != null && roomLayout[i,j] != currentRoom)
                    {
                        interactablesToCheck = roomLayout[i, j].GetInteractables();

                        foreach (AudienceInteractable audInt in interactablesToCheck)
                        {
                            if (audInt.GetID() == ID && audInt.state == state)
                            {
                                interactableOptions.Add(audInt);
                            }
                        }
                    }
                }
            }

            if (interactableOptions.Count > 0)
            {
                // Pick one
                int randomIndex = Random.Range(0, interactableOptions.Count);

                return interactableOptions[randomIndex];
            }
        }

        return null;
    }
}
