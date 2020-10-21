using System;
using System.Collections.Generic;
using UnityEngine;

public class MainObjective : MonoBehaviour, IInteractable
{
    public static event Action<IInteractable, bool> ActionPlayerClose = delegate { }; // This is sent to the player
    public static event Action<string, Vector3> ActionCollected = delegate { };
    public static event Action ActionLockDown = delegate { };

    public SideObjectiveType type;
    public float screenShakeAmount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActionPlayerClose(this, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActionPlayerClose(this, false);
        }
    }

    public void InteractWith(PlayerController interactingPlayer)
    {
        // Add money to player
        ActionCollected(SideObjective.StringFromType(type), transform.position);
        ActionLockDown();

        SoundRequest.RequestSound("Collect");
        SoundRequest.RequestSound("Lockdown");
        SoundRequest.RequestMusic("Intense_Music");
        Camera.main.GetComponent<ScreenShake>().Shake(screenShakeAmount);

        // Cleanup
        ActionPlayerClose(this, false);
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
