using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridSpace { empty, edge, floor, secure, gala, civilian, objective };

public class LevelGeneration : MonoBehaviour
{
    
    private GridSpace[,] grid;
    private int roomHeight, roomWidth;
    public Vector2 roomSizeWorldUnits = new Vector2(30, 30);
    private readonly float worldUnitsPerGridCell = 1;

    private struct Walker
    {
        public Vector2 dir;
        public Vector2 pos;
        public Vector2[] previousDirs;
        public int iterationsAlive;
    }

    private List<Walker> walkers;

    public float chanceWalkerChangeDir = 0.5f, chanceWalkerSpawn = 0.05f;
    public float chanceWalkerKillsSelf = 0.05f;
    public int maxWalkers = 5;
    public float percentToFill = 0.45f;
    public float percentCivilian = 0.166f;

    public void CreateLayout()
    {
        Setup();
        CreateFloors();
        DefineEdges();
        DefineFloors();
    }

    public GridSpace[,] GetGridSpaces()
    {
        return grid;
    }

    public int GetSecureRoomNum()
    {
        int totalRooms = Mathf.RoundToInt(roomWidth * roomHeight * percentToFill);
        return Mathf.RoundToInt(totalRooms - (totalRooms * percentCivilian) - 4);
    }

    private void Setup()
    {
        roomHeight = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsPerGridCell);
        roomWidth = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsPerGridCell);

        grid = new GridSpace[roomWidth, roomHeight];

        for (int i = 0; i < roomWidth - 1; i++)
        {
            for (int j = 0; j < roomHeight - 1; j++)
            {
                grid[i, j] = GridSpace.empty;
            }
        }

        walkers = new List<Walker>();

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                Vector2 spawnPos = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f - i), Mathf.RoundToInt(roomHeight / 2.0f + j));

                Walker newWalker = new Walker
                {
                    dir = RandomDirection(Vector2.zero),
                    pos = spawnPos,
                    iterationsAlive = 0
                };

                newWalker.previousDirs = new Vector2[4];

                walkers.Add(newWalker);
            }
        }
    }

    private void CreateFloors()
    {
        int iterations = 0;

        do
        {
            // create the floor at position of every walker
            foreach (Walker walker in walkers)
            {
                if (grid[(int)walker.pos.x, (int)walker.pos.y] != GridSpace.floor)
                {
                    grid[(int)walker.pos.x, (int)walker.pos.y] = GridSpace.floor;
                }
            }

            int walkersToCheck = walkers.Count;

            // Destroy Walkers
            for (int i = 0; i < walkersToCheck; ++i)
            {
                if (Random.value < chanceWalkerKillsSelf && walkers.Count > 1)
                {
                    walkers.RemoveAt(i);
                    break; //only remove one walker per iteration
                }
            }

            // Rotate Walkers
            for (int i = 0; i < walkers.Count; ++i)
            {
                Walker currentWalker = walkers[i];

                if (Random.value < chanceWalkerChangeDir)
                {
                    currentWalker.dir = RandomDirection(currentWalker.dir);
                }

                walkers[i] = currentWalker;
            }

            // Spawn New Walkers
            walkersToCheck = walkers.Count;

            for (int i = 0; i < walkersToCheck; ++i)
            {
                if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers)
                {
                    Walker nextWalker = new Walker
                    {
                        dir = RandomDirection(Vector2.zero),
                        pos = walkers[i].pos,
                        previousDirs = new Vector2[4]
                    };


                    walkers.Add(nextWalker);
                }
            }

            // Move Walkers
            for (int i = 0; i < walkers.Count; ++i)
            {
                Walker thisWalker = walkers[i];
                thisWalker.pos += thisWalker.dir;

                thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 1, roomWidth - 2);
                thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 1, roomHeight - 2);

                walkers[i] = thisWalker;
            }

            if (NumberOfFloors() / (float)grid.Length >= percentToFill)
            {
                break;
            }


            iterations++;
        } while (iterations < 100);
    }

    private void DefineEdges()
    {
        for (int i = 0; i < roomWidth; i++)
        {
            for (int j = 0; j < roomHeight; j++)
            {
                if (grid[i, j] == GridSpace.floor)
                {
                    // Check Top
                    if (j < roomHeight - 1)
                    {
                        if (grid[i, j + 1] == GridSpace.empty)
                        {
                            grid[i, j + 1] = GridSpace.edge;
                        }
                    }

                    // Check Right
                    if (i < roomWidth - 1)
                    {
                        if (grid[i + 1, j] == GridSpace.empty)
                        {
                            grid[i + 1, j] = GridSpace.edge;
                        }
                    }

                    // Check Bottom
                    if (j > 0)
                    {
                        if (grid[i, j - 1] == GridSpace.empty)
                        {
                            grid[i, j - 1] = GridSpace.edge;
                        }
                    }

                    // Check Left Wall
                    if (i > 0)
                    {
                        if (grid[i - 1, j] == GridSpace.empty)
                        {
                            grid[i - 1, j] = GridSpace.edge;
                        }
                    }
                }
            }
        }
    }

    private void DefineFloors()
    {
        // Set Up Gala Rooms
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                Vector2 galaRoomPos = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f - i), Mathf.RoundToInt(roomHeight / 2.0f + j));

                grid[(int)galaRoomPos.x, (int)galaRoomPos.y] = GridSpace.gala;
            }
        }

        // Determine number of civilian rooms to generate
        int numCivilian = Mathf.RoundToInt((roomHeight * roomWidth) * percentToFill * percentCivilian);
        int count = 0;

        while (count < numCivilian)
        {
            int x = Random.Range(0, roomWidth);
            int y = Random.Range(0, roomHeight);

            if (grid[x,y] == GridSpace.floor)
            {
                grid[x, y] = GridSpace.civilian;

                count++;
            }
        }

        // Pick Objective Room
        {
            int xPos, yPos;

            do
            {
                xPos = Random.Range(0, roomWidth);
                yPos = Random.Range(0, roomHeight);
            } while (grid[xPos, yPos] != GridSpace.floor);

            grid[xPos, yPos] = GridSpace.objective;
        }


        for (int i = 0; i < roomWidth; ++i)
        {
            for (int j = 0; j < roomHeight; ++j)
            {
                if (grid[i,j] == GridSpace.floor)
                {
                    grid[i, j] = GridSpace.secure;
                }
            }
        }
    }

    private Vector2 RandomDirection(Vector2 currentDir)
    {
        Vector2 newDir = Vector3.zero;

        do
        {
            int random = Random.Range(0, 4);

            switch (random)
            {
                case 0:
                    {
                        newDir = Vector2.down;
                    }
                    break;
                case 1:
                    {
                        newDir = Vector2.left;
                    }
                    break;
                case 2:
                    {
                        newDir = Vector2.up;
                    }
                    break;
                case 3:
                    {
                        newDir = Vector2.right;
                    }
                    break;
                default:
                    {
                        Debug.LogError("Bad Direction");
                        newDir = Vector2.zero;
                    }
                    break;
            }
        } while (currentDir == newDir);

        return newDir;
    }

    private int NumberOfFloors()
    {
        int count = 0;

        foreach (GridSpace space in grid)
        {
            if (space == GridSpace.floor)
            {
                count++;
            }
        }

        return count;
    }
}
