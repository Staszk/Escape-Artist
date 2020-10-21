using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Healthpack : MonoBehaviour, IInteractable
{
    public static event Action<IInteractable, bool> ActionPlayerClose = delegate { };
    public static event Action ActionHealthPackUsed = delegate { };

    public int healAmount;
    public int healPerTick = 4;

    public Canvas canvas;

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

            canvas.gameObject.SetActive(false);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void InteractWith(PlayerController interactingPlayer)
    {
        PlayerHealth health = interactingPlayer.GetComponent<PlayerHealth>();

        if (health.GetCurrHealth() < health.maxHealth)
        {
            // Play Sound
            SoundRequest.RequestSound("Heal");

            health.HealPlayer(interactingPlayer, healAmount, healPerTick);

            ActionHealthPackUsed();

            ActionPlayerClose(this, false);

            Destroy(gameObject);
        }
        else
        {
            SoundRequest.RequestSound("Deny");
            canvas.gameObject.SetActive(true);
            ActionPlayerClose(this, false);
        }
    }
}
