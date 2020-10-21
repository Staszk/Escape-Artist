using System.Collections.Generic;
using UnityEngine;

public class Stats
{
    public float runTime = 0f;

    // Stealthy
    public int lootInStealth = 0;  // TRACKED
    public int guardStealthTakedown = 0; // TRACKED

    // Reckless
    public int detected = 0;
    public int lootOutStealth = 0;  // TRACKED  -- NOT USED
    public int doorsForcedOpen = 0; // TRACKED -- NOT USED
        // ADD Camera

    // Combative
    public int attacks = 0;         // TRACKED
    public int healthPacksUsed = 0; // TRACKED

    // Other
    public int guardsKnockedOut = 0; // TRACKED
    public int damageDealt = 0;
    public int damageTaken = 0;
    public List<LootObject> objectList; // TRACKED

    // Audience -- ALL TRACKED
    public int goodDoors = 0;
    public int badDoors = 0;
    public int goodCameras = 0;
    public int badCameras = 0;
    public int goodTripwires = 0;
    public int badTripwires = 0;
    public int goodGuards = 0;
    public int badGuards = 0;
}

public struct LootObject
{
    public LootObject(string name, int price)
    {
        typeName = name;
        this.price = price;
        ID = -1;

        AssignID();
    }

    public string typeName;
    public int price;
    public int ID;

    private void AssignID()
    {
        switch (typeName)
        {
            case "Paintings":
                ID = 0;
                break;
            case "Statues":
                ID = 1;
                break;
            case "Artifact":
                ID = 2;
                break;
            case "Vase":
                ID = 3;
                break;
            case "Masks":
                ID = 4;
                break;
            case "Jewelry":
                ID = 5;
                break;
            case "Gem":
                ID = 6;
                break;
            case "MainArtPiece":
                ID = 7;
                break;
            default:
                break;
        }
    }
}

public class StatisticTracker
{
    private Stats stats;

    private readonly float startTime;

    private void Created()
    {
        KnockoutChecker.ActionEnemyKnockedOut += AddTakedown;
        EnemyHealth.ActionDamageHappened += AddAttack;
        EnemyHealth.ActionTakeDamage += AddDamageDealt;
        PlayerHealth.ActionTakeDamage += AddDamageTaken;
        Health.ActionEnemyKnockedOut += AddKnockOut;
        ForceOpenDoor.ActionDoorForcedOpen += AddDoorForcedOpen;
        Healthpack.ActionHealthPackUsed += AddHealthPackUse;

        TripwireInteractable.ActionLineCrossed += AddDetected;
        SecureRoom.ActionStartTimer += AddDetected;
    }

    private void Destroyed()
    {
        KnockoutChecker.ActionEnemyKnockedOut -= AddTakedown;
        EnemyHealth.ActionDamageHappened -= AddAttack;
        EnemyHealth.ActionTakeDamage -= AddDamageDealt;
        PlayerHealth.ActionTakeDamage -= AddDamageTaken;
        Health.ActionEnemyKnockedOut -= AddKnockOut;
        ForceOpenDoor.ActionDoorForcedOpen -= AddDoorForcedOpen;
        Healthpack.ActionHealthPackUsed -= AddHealthPackUse;

        TripwireInteractable.ActionLineCrossed -= AddDetected;
        SecureRoom.ActionStartTimer -= AddDetected;
    }

    public StatisticTracker()
    {
        Created();

        startTime = Time.time;

        stats = new Stats
        {
            objectList = new List<LootObject>()
        };
    }

    private void AddTakedown()
    {
        stats.guardStealthTakedown++;
    }

    private void AddKnockOut()
    {
        stats.guardsKnockedOut++;
    }

    private void AddAttack()
    {
        stats.attacks++;
    }

    private void AddDetected()
    {
        //Debug.Log("Stat: Detected");

        stats.detected++;
    }

    private void AddHealthPackUse()
    {
        stats.healthPacksUsed++;
    }

    private void AddDamageTaken(int damage)
    {
        stats.damageTaken += damage;
    }

    private void AddDamageDealt(int damage)
    {
        stats.damageDealt += damage;
    }

    private void AddDoorForcedOpen()
    {
        stats.doorsForcedOpen++;
    }

    public void AddInteractable(int ID, bool isGoodTeam)
    {
        switch (ID)
        {
            case 1:
                {
                    if (isGoodTeam)
                    {
                        stats.goodDoors++;
                    }
                    else
                    {
                        stats.badDoors++;
                    }
                }
                break;
            case 0:
                {
                    if (isGoodTeam)
                    {
                        stats.goodCameras++;
                    }
                    else
                    {
                        stats.badCameras++;
                    }
                }
                break;
            case 7:
                {
                    if (isGoodTeam)
                    {
                        stats.goodTripwires++;
                    }
                    else
                    {
                        stats.badTripwires++;
                    }
                }
                break;
            case 3:
                {
                    if (isGoodTeam)
                    {
                        stats.goodGuards++;
                    }
                    else
                    {
                        stats.badGuards++;
                    }
                }
                break;
            default:
                break;
        }
    }

    public void AddLootObject(string name, int price, bool stealInStealth)
    {
        if (stealInStealth)
        {
            stats.lootInStealth++;
        }
        else
        {
            stats.lootOutStealth++;
        }

        stats.objectList.Add(new LootObject(name, price));
    }

    public Stats GetStats()
    {
        // Get Final Time
        float finalTime = Time.time;

        stats.runTime = finalTime - startTime;

        Destroyed();

        return stats;
    }
}