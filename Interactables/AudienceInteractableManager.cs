using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using InControl;
using System;
using UnityEngine.UI;

public interface IGreyHatInteractable
{
    void Execute();
}

[Serializable]
public class InteractableData
{
    public string tag;
    public Sprite icon;
    public int ID;
    public InteractableState necessaryState;
}

[RequireComponent(typeof(LocateMainObjectiveInteractable))]
public class AudienceInteractableManager : MonoBehaviour
{
    public static event Action<int, bool> ActionAddInteractable = delegate { };
    public static event Action<InteractableData[], InteractableData[]> ActionHasNewOptions = delegate { };
    public static event Action<bool, Sprite> ActionFlashInteractable = delegate { };
    public static event Action<Vector3, bool> ActionInteractableChosen = delegate { };
    public static event Action<int, int, string> ActionLockFaction = delegate { };

    public InteractableData[] goodInteractables;
    public InteractableData[] badInteractables;

    private InteractableData[] goodChosenData;
    private InteractableData[] badChosenData;

    private float[] goodDataValues;
    private float[] badDataValues;
    private bool goodFactionLocked;
    private bool badFactionLocked;

    private readonly float tickValue = 0.025f;
    private int[] audienceMembers;
    public InteractableUIBox[] interactableUI;

    public float timeBetweenOptions;
    public float timeToSpam;
    private float timer;

    private bool interactablesActive;

    private InputDevice[] audienceControllers;

    private Vector2 currentRoomLocation;

    public bool includeMainObjective;

    public bool readyToPlay;

    public void InitializeInteractables()
    {
        goodDataValues = new float[3];
        badDataValues = new float[3];

        includeMainObjective = true;

        audienceMembers = new int[2];
        audienceMembers[0] = 1;
        audienceMembers[1] = 1;

        interactablesActive = false;

        audienceControllers = new InputDevice[2];

        foreach (InteractableUIBox box in interactableUI)
        {
            box.UpdateBox(false);
        }

        goodFactionLocked = false;
        badFactionLocked = false;

        readyToPlay = false;
    }

    private void Update()
    {
        if (interactablesActive && readyToPlay)
        {
            timer += Time.deltaTime;

            if (timer >= timeToSpam)
            {
                // Turn everything off
                TurnOffDisplay(0, -1);
                TurnOffDisplay(1, -1);

                if (!goodFactionLocked && !badFactionLocked)
                {
                    ActionLockFaction(2, 1, "Not Enough Votes");
                }
                else if (!badFactionLocked)
                {
                    ActionLockFaction(0, 1, "Not Enough Votes");
                }
                else if (!goodFactionLocked)
                {
                    ActionLockFaction(1, 1, "Not Enough Votes");
                }

                goodFactionLocked = false;
                badFactionLocked = false;

                // Reset Values for next iteration
                goodDataValues = new float[3];
                badDataValues = new float[3];

                StartCoroutine(GetNewOptions());

                interactablesActive = false;
                timer = 0;
            }
        }
    }

    public void SetReadyToPlay()
    {
        readyToPlay = true;
        StartCoroutine(GetNewOptions());
    }

    public void GetInputFromWebsite(int[] timesClickedGood, int[] timesClickedBad, int[] audienceNumbers)
    {
        // 0 is bad; 1 is good
        audienceMembers[0] = audienceNumbers[0];
        audienceMembers[1] = audienceNumbers[1];

        for (int i = 0; i < 3; i++)
        {
            // Apply clicks to good
            if (!goodFactionLocked)
                goodDataValues[i] += (tickValue * timesClickedGood[i] / audienceMembers[1]);

            // Apply clicks to bad
            if (!badFactionLocked)
                badDataValues[i] += (tickValue * timesClickedBad[i] / audienceMembers[0]);
        }

        CheckValues();
    }

    public void SetCurrentRoomLocation(int x, int z)
    {
        currentRoomLocation = new Vector2(x, z);
    }

    private void GetAudienceController(InputDevice[] controllers)
    {
        audienceControllers[0] = controllers[1];
        audienceControllers[1] = controllers[2];
    }

    private void CheckValues()
    {
        // Check Good Values
        for (int i = 0; i < 3; i++)
        {
            interactableUI[i].UpdateBox(goodDataValues[i]);

            if (goodDataValues[i] >= 1.0f)
            {
                // this is the winner
                int ID = goodChosenData[i].ID;
                InteractableState state = goodChosenData[i].necessaryState;

                ActionLockFaction(1, 1, goodChosenData[i].tag);
                goodFactionLocked = true;

                ActionFlashInteractable(true, goodChosenData[i].icon);

                ActionAddInteractable(goodChosenData[i].ID, true);
                
                // If the chosen interactable does not have a physical location
                if (ID == 4)
                {
                    GetComponent<LocateMainObjectiveInteractable>().Execute();
                    ActionInteractableChosen(RoomManager.instance.GetCurrentRoom().transform.position, true);

                    SoundRequest.RequestSound("Ping");
                }
                else
                {
                    // Get nearby object with that ID and State
                    AudienceInteractable chosenInteractable = RoomManager.instance.GetInteractableOfType(ID, state);

                    //ActionInteractableChosen
                    if (chosenInteractable != null)
                    {
                        chosenInteractable.Execute();
                        ActionInteractableChosen(chosenInteractable.transform.position, true);
                        SoundRequest.RequestSound("Ping");
                    }
                    else
                    {
                        Debug.LogError("No Interactable Possible");
                    }
                }

                TurnOffDisplay(0, i); // Turns off all except for the given display
                goodDataValues = new float[] { 0.0f, 0.0f, 0.0f };
            }
        }

        // Check Bad Values
        for (int i = 0; i < 3; i++)
        {
            interactableUI[i + 3].UpdateBox(badDataValues[i]);

            if (badDataValues[i] >= 1.0f)
            {
                if (badDataValues[i] >= 1.0f)
                {
                    // this is the winner
                    int ID = badChosenData[i].ID;
                    InteractableState state = badChosenData[i].necessaryState;

                    // Get nearby object with that ID and State
                    AudienceInteractable chosenInteractable = RoomManager.instance.GetInteractableOfType(ID, state);

                    ActionLockFaction(0, 1, badChosenData[i].tag);
                    ActionFlashInteractable(false, badChosenData[i].icon);
                    badFactionLocked = true;

                    ActionAddInteractable(badChosenData[i].ID, false);

                    //ActionInteractableChosen
                    if (chosenInteractable != null)
                    {
                        chosenInteractable.Execute();
                        ActionInteractableChosen(chosenInteractable.transform.position, false);
                        SoundRequest.RequestSound("Ping");
                    }
                    else
                    {
                        Debug.LogError("No Interactable Possible");
                    }

                    TurnOffDisplay(1, i); // Turns off all except for the given display
                    badDataValues = new float[] { 0.0f, 0.0f, 0.0f };
                }
            }
        }
    }

    private void TurnOffDisplay(int col, int row)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i != row)
            {
                int index = col * 3 + i;
                interactableUI[index].UpdateBox(false);
            }
        }
    }

    private void LoadNewInteractableUI()
    {
        // Good Interactions
        for (int i = 0; i < 3; i++)
        {
            interactableUI[i].UpdateBox(0.0f, goodChosenData[i].tag, goodChosenData[i].icon);
        }

        // Bad Interactions
        for (int i = 3; i < 6; i++)
        {
            interactableUI[i].UpdateBox(0.0f, badChosenData[i - 3].tag, badChosenData[i - 3].icon);
        }
    }

    private IEnumerator GetNewOptions(bool wait = true)
    {
        // Happening first for website
        PickNewOptions();
        ActionHasNewOptions(goodChosenData, badChosenData);

        // Let website catch up, also wait for gameplay
        if (wait)
        {
            yield return new WaitForSeconds(2.0f);

            // Unlock Website
            ActionLockFaction(2, 0, "");

            yield return new WaitForSeconds(2.0f);
        }

        // Show the player
        LoadNewInteractableUI();
        interactablesActive = true;
    }

    private void PickNewOptions()
    {
        List<int> chosenIndexes = new List<int>();
        int randomChoice;

        goodChosenData = new InteractableData[3];
        badChosenData = new InteractableData[3];

        float chance = UnityEngine.Random.Range(0.0f, 1f);

        int range = (chance <= 0.35f && includeMainObjective) ? goodInteractables.Length : goodInteractables.Length - 1;

        for (int i = 0; i < 3; i++)
        {
            do
            {
                randomChoice = UnityEngine.Random.Range(0, range);
            } while (chosenIndexes.Contains(randomChoice));

            chosenIndexes.Add(randomChoice);

            goodChosenData[i] = goodInteractables[randomChoice];
        }

        chosenIndexes.Clear();

        for (int i = 0; i < 3; i++)
        {
            do
            {
                randomChoice = UnityEngine.Random.Range(0, badInteractables.Length);
            } while (chosenIndexes.Contains(randomChoice));

            chosenIndexes.Add(randomChoice);

            badChosenData[i] = badInteractables[randomChoice];
        }
    }
}
