using UnityEngine;
using System.Collections.Generic;
using System;

public class SecureRoom : Room
{
    public static event Action<Enemy> ActionEnemyCreated = delegate { };
    public static event Action ActionStartTimer = delegate { };

    public GameObject guardWalkNodes;
    private List<Transform> nodes;
    public List<Transform> guardSpawnNodes;

    public List<SideObjectiveSpawnNode> sideObjectiveSpawns;

    private List<Enemy> enemiesOwnedByRoom;

    private GameObject enemyPrefab;

    public bool hasAlerted;

    private GameObject alertQuad;

    public override void Setup(int x, int z, string name, RoomType typeOfRoom, bool[] doors, GameObject[] wallObjects)
    {
        base.Setup(x, z, name, RoomType.Secure, doors, wallObjects);

        hasAlerted = false;
        enemyPrefab = null;

        AudienceInteractable temp = gameObject.AddComponent<EnemySpawnInteractable>();
        InitAudienceInteractable(temp);
        audienceInteractables.Add(temp);

        temp = gameObject.AddComponent<EnemyIncapacitateInteractable>();
        InitAudienceInteractable(temp);
        audienceInteractables.Add(temp);

        CheckInteractablesInRoom();

        nodes = new List<Transform>();

        foreach (Transform child in guardWalkNodes.transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                nodes.Add(child);
            }
        }

        enemiesOwnedByRoom = new List<Enemy>();

        MainObjective.ActionLockDown += NotifyEnemiesOfPlayer;
    }

    private void OnDisable()
    {
        MainObjective.ActionLockDown -= NotifyEnemiesOfPlayer;
    }

    public void GiveQuad(GameObject quad)
    {
        alertQuad = Instantiate(quad, transform);
        alertQuad.SetActive(false);
    }

    private void CheckInteractablesInRoom()
    {
        Transform firstChild = transform.GetChild(0);

        foreach (Transform t in firstChild)
        {
            AudienceInteractable audInt = t.GetComponent<AudienceInteractable>();

            if (audInt != null)
            {
                InitAudienceInteractable(audInt);
            }
        }
    }

    public void SpawnSideObjective()
    {
        foreach (SideObjectiveSpawnNode spawn in sideObjectiveSpawns)
        {
            if (spawn != null)
            {
                spawn.SpawnSideObjective();
            }
            else
            {
                Debug.Log(name);
            }
        }
    }

    public void AlertCameras()
    {
        foreach (AudienceInteractable audInt in audienceInteractables)
        {
            CameraInteractable cam = audInt.GetComponent<CameraInteractable>();

            if (cam != null)
            {
                cam.SetAlert();
            }
        }
    }

    public void CleanUpSideObjectiveNodes()
    {
        foreach (SideObjectiveSpawnNode node in sideObjectiveSpawns)
        {
            Destroy(node.gameObject);
        }
    }

    public void SpawnGuard(GameObject enemyPrefab)
    {
        this.enemyPrefab = enemyPrefab;

        SpawnGuard();
    }

    public void SpawnGuard()
    {
        int random = UnityEngine.Random.Range(0, guardSpawnNodes.Count);

        Enemy newEnemy = Instantiate(enemyPrefab, guardSpawnNodes[random].position, guardSpawnNodes[random].rotation).GetComponent<Enemy>();

        // Set Up Enemy
        newEnemy.InitializeEnemy(3, nodes, this);

        ActionEnemyCreated(newEnemy);
        enemiesOwnedByRoom.Add(newEnemy);

        if(hasAlerted)
        {
            newEnemy.NotifyOfPlayer();
        }
        if (FindObjectOfType<CameraSmoothFollow>().GetCurrentRoom() == this)
        {
            newEnemy.SetPlayerInRoom(true);
        }
    }

    public void UpdatePlayerInRoom(bool playerInRoom)
    {
        foreach (Enemy e in enemiesOwnedByRoom)
        {
            e.SetPlayerInRoom(playerInRoom);
        }
    }

    public void NotifyEnemiesOfPlayer()
    {
        hasAlerted = true;

        alertQuad.SetActive(true);

        SoundRequest.RequestSound("Surprised");

        foreach (Enemy e in enemiesOwnedByRoom)
        {
            e.NotifyOfPlayer();
        }

        AlertCameras();
    }

    public void RoomAlerted()
    {
        if (!hasAlerted)
        {
            ActionStartTimer();
        }
    }

    public void TurnCamerasOnOff(bool turnOn)
    {
        foreach (AudienceInteractable audInt in audienceInteractables)
        {
            CameraInteractable cam = audInt.GetComponent<CameraInteractable>();

            if (cam != null)
            {
                cam.SetOnOff(turnOn);
            }
        }
    }

    public void TurnTripwiresOnOff(bool turnOn)
    {
        foreach (AudienceInteractable audInt in audienceInteractables)
        {
            TripwireInteractable wire = audInt.GetComponent<TripwireInteractable>();

            if (wire != null)
            {
                wire.SetOnOff(turnOn);
            }
        }
    }

    public void EMPGuards()
    {
        for(int i = 0; i < enemiesOwnedByRoom.Count; ++i)
        {
            enemiesOwnedByRoom[i].GetComponent<EnemyHealth>().ZeroOutHealth();
        }
    }
}
