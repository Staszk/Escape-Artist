using System;
using UnityEngine;
using UnityEngine.UI;

public class AlertSystem : MonoBehaviour
{
    public static event Action<int> ActionAlertChanged = delegate { };
    public static event Action ActionLockdownMode = delegate { };

    public LayerMask enemyLayer;

    public AlertMeter alertMeter;

    private bool alertedInRoom;
    private bool inLockDown;

    private int minAlertLevel;
    private int currentAlertLevel;
    private int maxAlertLevel;

    private float timeToDecreaseAlert;
    private float decreaseTimer;

    private void Start()
    {
        currentAlertLevel = minAlertLevel = 0;
        maxAlertLevel = 4;

        timeToDecreaseAlert = 60;
        decreaseTimer = 0;

        alertedInRoom = false;
        inLockDown = false;
    }

    private void OnEnable()
    {
        Enemy.ActionSawPlayer += IncreaseAlert;
        CameraSmoothFollow.ActionMovedRoom += ChangeRoom;
        CameraInteractable.ActionSawPlayer += IncreaseAlert;
        TripwireInteractable.ActionLineCrossed += IncreaseAlert;
        MainObjective.ActionLockDown += MakeLockDown;
        PlayerController.ActionCreateNoise += CreateSoundAtPoint;
    }

    private void OnDisable()
    {
        Enemy.ActionSawPlayer -= IncreaseAlert;
        CameraSmoothFollow.ActionMovedRoom -= ChangeRoom;
        CameraInteractable.ActionSawPlayer -= IncreaseAlert;
        TripwireInteractable.ActionLineCrossed -= IncreaseAlert;
        MainObjective.ActionLockDown -= MakeLockDown;
        PlayerController.ActionCreateNoise -= CreateSoundAtPoint;
    }

    private void Update()
    {
        if (!inLockDown && currentAlertLevel > minAlertLevel)
        {
            decreaseTimer += Time.deltaTime;

            if (decreaseTimer >= timeToDecreaseAlert)
            {
                DecreaseAlert();
                decreaseTimer = 0;
            }
        }
    }


    private void CreateSoundAtPoint(Vector3 origin, float noiseValue)
    {
        float radius = noiseValue / 4.0f;

        Collider[] hitColliders = Physics.OverlapSphere(origin, radius, enemyLayer);

        foreach (Collider col in hitColliders)
        {
            // Send noise to enemy
        }
    }

    private void MakeLockDown()
    {
        currentAlertLevel = 4;

        ActionAlertChanged(currentAlertLevel);

        alertMeter.SetMaxAlert();

        inLockDown = true;

        ActionLockdownMode();
    }

    private void IncreaseAlert()
    {
        if (!alertedInRoom && currentAlertLevel < maxAlertLevel)
        {
            ++currentAlertLevel;

            alertedInRoom = true;

            ActionAlertChanged(currentAlertLevel);

            alertMeter.IncreaseAlert();

            if(currentAlertLevel == 4)
            {
                inLockDown = true;
                ActionLockdownMode();
            }
        }
    }

    private void DecreaseAlert()
    {
        if (currentAlertLevel > minAlertLevel)
        {
            --currentAlertLevel;
            ActionAlertChanged(currentAlertLevel);

            alertMeter.ReduceAlert();

            // Possibly redundant, but might come into effect later
            decreaseTimer = 0; 
        }
    }

    private void ChangeRoom()
    {
        alertedInRoom = false;
    }
}
