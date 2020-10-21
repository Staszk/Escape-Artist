using System;
using UnityEngine;

public enum SideObjectiveType
{
    mainArt,
    mask,
    statue,
    artifact,
    vase,
    paintings,
    jewelry,
    gem
}

public class SideObjective : MonoBehaviour, IInteractable
{
    public static event Action<IInteractable, bool> ActionPlayerClose = delegate { }; // This is sent to the player
    public static event Action<string, Vector3> ActionCollected = delegate { };

    public SideObjectiveType type;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
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
        ActionCollected(StringFromType(type), transform.position);
        SoundRequest.RequestSound("Collect");

        // Cleanup
        ActionPlayerClose(this, false);
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public static string StringFromType(SideObjectiveType type)
    {
        switch (type)
        {
            case SideObjectiveType.mainArt:
                return "MainArtPiece";
            case SideObjectiveType.mask:
                return "Masks";
            case SideObjectiveType.statue:
                return "Statues";
            case SideObjectiveType.artifact:
                return "Artifact";
            case SideObjectiveType.vase:
                return "Vase";
            case SideObjectiveType.paintings:
                return "Paintings";
            case SideObjectiveType.jewelry:
                return "Jewelry";
            case SideObjectiveType.gem:
                return "Gem";
            default:
                return "none";
        }
    }
}

public interface IInteractable
{
    void InteractWith(PlayerController interactingPlayer);

    Vector3 GetPosition();
}

