using UnityEngine;
using System.Collections;
using InControl;
using System.Linq;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class EndMenuSystem : MonoBehaviour
{
    private InputDevice controller;

    private readonly string escapedString = "You <color=#E39C00>Escaped</color>";
    private readonly string caughtString = "You Were <color=#ff0000>Caught</color>";

    private readonly float maxPercent = 0.875f;
    private readonly float minPercent = 0.125f;

    public TMP_Text titleText;

    public TextStatBlock timeStat;
    public TextStatBlock guardsStat;
    public TextStatBlock doorsForcedStat;
    public TextStatBlock healthPacksStat;

    public SliderStatBlock guardSlider;
    public SliderStatBlock doorSlider;
    public SliderStatBlock cameraSlider;
    public SliderStatBlock tripwireSlider;

    public PieChart chart;

    public Transform grid;
    public GameObject[] imageObjects;
    public TMP_Text moneyText;

    private bool skipPress = false;
    private bool canTransition = false;

    private Stats gameStats;

    private void OnEnable()
    {
		ControllerInputSystem.ActionEndScreenInput += UpdateController;
    }

    private void OnDisable()
    {
        ControllerInputSystem.ActionEndScreenInput -= UpdateController;
    }

    private void Update()
    {
        if (controller.Action1.WasPressed)
        {
            if (!skipPress)
            {
                skipPress = true;
            }
            else
            {
                // Go To First Scene
                //SceneManager.LoadScene(0);
                Time.timeScale = 1;
                canTransition = true;
            }
        }
    }

    private void UpdateController(InputDevice device)
    {
        controller = device;
    }

    public void PrepareScreen(bool goodResult, Stats stats)
    {
        titleText.text = goodResult ? escapedString : caughtString;

        #region Stats

        // Time
        float time = stats.runTime;
        int minutes = Mathf.RoundToInt(time) / 60;
        int seconds = Mathf.RoundToInt(time) % 60;

        // Guards
        int guardsKnockedOut = stats.guardStealthTakedown + stats.guardsKnockedOut;

        timeStat.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
        guardsStat.SetText(guardsKnockedOut.ToString());
        doorsForcedStat.SetText(stats.damageDealt.ToString());
        healthPacksStat.SetText(stats.damageTaken.ToString());

        // Slider Stats
            // Guards
        int totalGuardWins = stats.goodGuards + stats.badGuards;
        float goodGuardPercent = 0.5f;
        float badGuardPercent = 0.5f;

        if (totalGuardWins > 0)
        {
            goodGuardPercent = Mathf.Clamp((float)stats.goodGuards / totalGuardWins, minPercent, maxPercent);
            badGuardPercent = Mathf.Clamp((float)stats.badGuards / totalGuardWins, minPercent, maxPercent);
        }

            // Doors
        int totalDoorWins = stats.goodDoors + stats.badDoors;
        float goodDoorPercent = 0.5f;
        float badDoorPercent = 0.5f;

        if (totalDoorWins > 0)
        {
            goodDoorPercent = Mathf.Clamp((float)stats.goodDoors / totalDoorWins, minPercent, maxPercent);
            badDoorPercent = Mathf.Clamp((float)stats.badDoors / totalDoorWins, minPercent, maxPercent);
        }

            // Cameras
        int totalCameraWins = stats.goodCameras + stats.badCameras;
        float goodCameraPercent = 0.5f;
        float badCameraPercent = 0.5f;

        if (totalCameraWins > 0)
        {
            goodCameraPercent = Mathf.Clamp((float)stats.goodCameras / totalCameraWins, minPercent, maxPercent);
            badCameraPercent = Mathf.Clamp((float)stats.badCameras / totalCameraWins, minPercent, maxPercent);
        }

            // Tripwires
        int totalTripwireWins = stats.goodTripwires + stats.badTripwires;
        float goodTripwirePercent = 0.5f;
        float badTripwirePercent = 0.5f;
        
        if (totalTripwireWins > 0)
        {
            goodTripwirePercent = Mathf.Clamp((float)stats.goodTripwires / totalTripwireWins, minPercent, maxPercent);
            badTripwirePercent = Mathf.Clamp((float)stats.badTripwires / totalTripwireWins, minPercent, maxPercent);
        }

        guardSlider.SetUp(goodGuardPercent, badGuardPercent, stats.goodGuards, stats.badGuards);
        doorSlider.SetUp(goodDoorPercent, badDoorPercent, stats.goodDoors, stats.badDoors);
        cameraSlider.SetUp(goodCameraPercent, badCameraPercent, stats.goodCameras, stats.badCameras);
        tripwireSlider.SetUp(goodTripwirePercent, badTripwirePercent, stats.goodTripwires, stats.badTripwires);

        #endregion

        #region Play Style

        int stealthy = stats.lootInStealth + stats.guardStealthTakedown;
        int reckless = stats.detected;
        int combative = stats.attacks + stats.healthPacksUsed;
        int total = stealthy + reckless + combative;

        float[] percents = new float[3];

        if (total != 0)
        {
            percents[0] = (float)stealthy / total;
            percents[1] = (float)reckless / total;
            percents[2] = (float)combative / total;
        }
        else
        {
            percents[0] = (float)1/3;
            percents[1] = (float)1/3;
            percents[2] = (float)1/3;
        }

        int index = Array.IndexOf(percents, percents.Max());

        chart.SetUp(percents, index);

        #endregion

        #region Loot

        stats.objectList.Sort(SortByID);

        gameStats = stats;

        #endregion
    }

    static int SortByID(LootObject lhs, LootObject rhs)
    {
        return lhs.ID.CompareTo(rhs.ID);
    }

    public void StartImages()
    {
        SoundRequest.RequestMusic("End_Music");
        StartCoroutine(ShowObjectives());
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        // Wait One Frame
        yield return null;

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(0);
        asyncOp.allowSceneActivation = false;

        while (!asyncOp.isDone)
        {
            if (asyncOp.progress >= 0.90f)
            {
                if (canTransition)
                {
                    asyncOp.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    private IEnumerator ShowObjectives()
    {
        int count = 0;
        int max = gameStats.objectList.Count;
        int moneyCount = 0;

        moneyText.text = "$" + moneyCount.ToString();

        while (count < max)
        {
            if (!skipPress)
            {
                yield return new WaitForSecondsRealtime(0.075f);
                SoundRequest.RequestSound("Punch");
            }

            // Spawn next object in list

            LootObject lo = gameStats.objectList[count];

            Instantiate(ImageFromID(lo.ID), grid);

			moneyCount += lo.price;

            moneyText.text = "$" + moneyCount.ToString();

            count++;
        }

        skipPress = true;
    }

    private GameObject ImageFromID(int ID)
    {
        return imageObjects[ID];
    }
}
