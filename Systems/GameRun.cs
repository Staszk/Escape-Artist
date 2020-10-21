using System.Collections;
using UnityEngine;

public class GameRun : MonoBehaviour
{
    private LevelGeneration levelGenerator;
    private RoomManager roomManager;
    private AudienceInteractableManager audienceManager;
    private CameraSmoothFollow cameraFollow;
    private MinimapCamera minimapCamera;
    private ScreenUISystem screenUI;
    private ControllerInputSystem controllerInput;
    private StatisticTracker statTracker;

    private PlayerController player;

    private EnemyManager enemyManager;

    private WebsiteScript websiteData;

    private readonly float timeBetweenWebsiteRequests = 1.0f;
    private float timer;

    public bool connectWebsite;

    // Game Variables
    private Room currentRoom;
    int moneyCollected;

    //Lockdown Timer Variables
    //private float runTimer;
    //private bool timerActive;
    //private bool switchedRooms;

    public void Initialize()
    {
        controllerInput = FindObjectOfType<ControllerInputSystem>().GetComponent<ControllerInputSystem>();
        levelGenerator = GetComponent<LevelGeneration>();
        roomManager = GetComponent<RoomManager>();
        cameraFollow = GameObject.Find("Main Camera").GetComponent<CameraSmoothFollow>();
        minimapCamera = FindObjectOfType<MinimapCamera>();
        audienceManager = FindObjectOfType<AudienceInteractable>().GetComponent<AudienceInteractableManager>();

        websiteData = new WebsiteScript(connectWebsite);
        StartCoroutine(websiteData.LockFaction(2, 0, ""));
        screenUI = FindObjectOfType<ScreenUISystem>().GetComponent<ScreenUISystem>();

        enemyManager = new EnemyManager();



        // Setup level layout
        levelGenerator.CreateLayout();
        roomManager.SpawnLayout(levelGenerator.GetGridSpaces(), levelGenerator.GetSecureRoomNum());

        audienceManager.InitializeInteractables();

        player = FindObjectOfType<PlayerController>();

        timer = 0;

        // Game Variables
        moneyCollected = 0;

        //runTimer = 600;

        //switchedRooms = false;
    }

    private void OnEnable()
    {
        StartButton.ActionStartButtonPressed += StartRun;

        AudienceInteractableManager.ActionHasNewOptions += UpdateWebsiteOptions;
        AudienceInteractableManager.ActionLockFaction += LockWebsite;
        WebsiteScript.ActionFinishedClickRequest += SendWebsiteDataToAudienceManager;
        WebsiteScript.ActionGotPrice += AddValueToTotal;
        PlayerStartRoom.ActionPlayerTransformSpawned += PlayerSpawned;
        RoomTrigger.ActionPlayerEnteredRegion += ChangeRoom;
        SecureRoom.ActionEnemyCreated += SendEnemyToManager;
        SecureRoom.ActionStartTimer += StartTimer;

        SideObjective.ActionCollected += GetPriceFromSite;
        MainObjective.ActionCollected += GetPriceFromSite;

        MainObjective.ActionLockDown += LockDownState;

        PlayerHealth.ActionPlayerDeath += EndRunDeath;
        EndGameTrigger.ActionEndGame += EndRunWin;
    }

    private void OnDisable()
    {
        StartButton.ActionStartButtonPressed -= StartRun;

        AudienceInteractableManager.ActionHasNewOptions -= UpdateWebsiteOptions;
        AudienceInteractableManager.ActionLockFaction -= LockWebsite;
        WebsiteScript.ActionGotPrice -= AddValueToTotal;
        WebsiteScript.ActionFinishedClickRequest -= SendWebsiteDataToAudienceManager;
        PlayerStartRoom.ActionPlayerTransformSpawned -= PlayerSpawned;
        RoomTrigger.ActionPlayerEnteredRegion -= ChangeRoom;
        SecureRoom.ActionEnemyCreated -= SendEnemyToManager;
        SecureRoom.ActionStartTimer -= StartTimer;

        SideObjective.ActionCollected -= GetPriceFromSite;
        MainObjective.ActionCollected -= GetPriceFromSite;

        MainObjective.ActionLockDown -= LockDownState;

        PlayerHealth.ActionPlayerDeath -= EndRunDeath;
        EndGameTrigger.ActionEndGame -= EndRunWin;
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(websiteData.LockFaction(2, 1, "GAME OVER"));
    }

    private void FixedUpdate()
    {
        if (connectWebsite)
        {
            timer += Time.deltaTime;
            if (timer >= timeBetweenWebsiteRequests)
            {
                AskWebsite();

                timer = 0f;
            }
        }

        enemyManager.UpdateEnemies();

        //if(timerActive)
        //{
        //    CountDown();
        //}
    }

    private void StartRun()
    {
        statTracker = new StatisticTracker();
    }

    private void CountDown()
    {
        //runTimer -= Time.deltaTime;

        //screenUI.GetGameScreen().ShowTimer(runTimer);

        //if(runTimer == 119)
        //{
        //    screenUI.GetGameScreen().Show60Left();
        //}

        //if (runTimer <= 0f)
        //{
        //    timerActive = false;
        //    EndRunDeath();
        //}
    }

    public void StartTimer()
    {
        //if (!timerActive)
        //{
        //    timerActive = true;
        //}
        //else if (!switchedRooms)
        //{
        //    runTimer -= 5;
        //    screenUI.GetGameScreen().ShowSubtractTimer();
        //}
    }

    public void LockDownTimer()
    {
        //if (runTimer > 119)
        //{
        //    runTimer = 119;
        //    screenUI.GetGameScreen().Show60Left();
        //}

        //timerActive = true;
    }

    private void LockDownState()
    {
        enemyManager.AlertEnemiesLockdown();
        audienceManager.includeMainObjective = false;
        LockDownTimer();
    }

    private void SendEnemyToManager(Enemy e)
    {
        enemyManager.TrackEnemy(e);
    }

    private void ChangeRoom(Room newRoom)
    {
        if (newRoom != currentRoom)
        {
            //switchedRooms = true;
            //StartCoroutine(JustSwitchedRooms());

            if (currentRoom != null && currentRoom.GetRoomType() == RoomType.Secure)
            {
                SecureRoom room = (SecureRoom)currentRoom;
                room.UpdatePlayerInRoom(false);
            }

            if (newRoom.GetRoomType() == RoomType.Secure)
            {
                SecureRoom room = (SecureRoom)newRoom;
                room.UpdatePlayerInRoom(true);

                player.SetKnockout(!room.hasAlerted);
            }

            currentRoom = newRoom;
            roomManager.ChangeRoom(currentRoom);
            cameraFollow.ChangeRoom(currentRoom);
            minimapCamera.MoveLocation(currentRoom);
            currentRoom.VisitRoom();
            audienceManager.SetCurrentRoomLocation(currentRoom.X, currentRoom.Z);
        }
    }

    private IEnumerator JustSwitchedRooms()
    {
        yield return new WaitForSeconds(0.8f);
        //switchedRooms = false;
    }

    private void AskWebsite()
    {
        StartCoroutine(websiteData.GetRequest());
    }

    private void LockWebsite(int team, int locked, string winner)
    {
        StartCoroutine(websiteData.LockFaction(team, locked, winner));
    }

    private void SendWebsiteDataToAudienceManager(int[] clicksGood, int[] clicksBad, int[] audienceNumbers)
    {
        audienceManager.GetInputFromWebsite(clicksGood, clicksBad, audienceNumbers);
    }

    private void UpdateWebsiteOptions(InteractableData[] goodData, InteractableData[] badData)
    {
        if (connectWebsite)
        {
            string[,] textToDisplay = new string[2, 3];

            // Good Names
            for (int i = 0; i < 3; i++)
            {
                textToDisplay[0, i] = goodData[i].tag;
            }


            // Bad names
            for (int i = 0; i < 3; i++)
            {
                textToDisplay[1, i] = badData[i].tag;
            }

            StartCoroutine(websiteData.UpdateInteractables(textToDisplay));
        }
    }

    private void GetPriceFromSite(string tag, Vector3 location)
    {
        StartCoroutine(websiteData.GetItemPrice(tag, location));
    }

    private void AddValueToTotal(string type, int value, int upOrDown, Vector3 location)
    {
        bool bonus = false;

        if (currentRoom is SecureRoom room)
        {
            int stealthCash = room.hasAlerted ? 0 : 200;

            bonus = !room.hasAlerted;

            value += stealthCash;
        }

        statTracker.AddLootObject(type, value, bonus);

        moneyCollected += value;

        screenUI.UpdateGameScreen(moneyCollected, value, bonus);

        screenUI.GetGameScreen().ShowPriceFlash(location, upOrDown);
    }

    private void PlayerSpawned(Transform playerTransform)
    {
        // Set Up Camera
        Room startRoom = roomManager.GetStartingRoom();
        cameraFollow.SetRoom(startRoom);
        minimapCamera.MoveLocation(startRoom);
        ChangeRoom(startRoom);
        cameraFollow.SetPlayerTransform(playerTransform);

        // Set up EnemyManager
        enemyManager.TrackPlayer(playerTransform);
    }

    public void BeginRun()
    {
        cameraFollow.ZoomInToPlayer();
    }

    private void EndRunWin(EndGameTrigger trigger)
    {
        if (!(moneyCollected > 0))
        {
            trigger.ShowCanvas(true);
        }
        else
        {
            enemyManager.DeleteEnemies();
            Destroy(player.gameObject);
            TransitionToEndScreen(true);
            StartCoroutine(websiteData.LockFaction(2, 1, "GAME OVER"));
        }
    }

    private void EndRunDeath()
    {
        enemyManager.DeleteEnemies();

        Destroy(player.gameObject);

        TransitionToEndScreen(false);

        StartCoroutine(websiteData.LockFaction(2, 1, "GAME OVER"));
    }

    private void TransitionToEndScreen(bool goodResult)
    {
        Time.timeScale = 0;

        Stats stats = statTracker.GetStats();

        controllerInput.ChangeState(GameState.End_Screen);
        screenUI.EndTransition(goodResult, stats);
    }
}
