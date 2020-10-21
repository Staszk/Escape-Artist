using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndGameTrigger : MonoBehaviour, IInteractable
{
    public static event Action<IInteractable, bool> ActionPlayerWithinRadius = delegate { };
    public static event Action<EndGameTrigger> ActionEndGame = delegate { };

    public GameObject canvas;

    public void InteractWith(PlayerController interactingPlayer)
    {
        ActionEndGame(this);
        interactingPlayer.currentInteractable = null;
    }

    public void ShowCanvas(bool show)
    {
        canvas.SetActive(show);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ActionPlayerWithinRadius(this, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ActionPlayerWithinRadius(this, false);
            ShowCanvas(false);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
