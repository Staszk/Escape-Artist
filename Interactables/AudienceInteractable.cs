using UnityEngine;

public enum InteractableState
{
    none,
    positive,
    negative
}

[System.Serializable]
public abstract class AudienceInteractable : MonoBehaviour, IGreyHatInteractable
{
    public new string name;
    public InteractableState state;
    protected Room parentRoom;

    protected int ID;

    public virtual void InitializeInteractable(string name, InteractableState state, Room parent)
    {
        parentRoom = parent;
        this.name = name;
        this.state = state;
    }

    public int GetID()
    {
        return ID;
    }

    public virtual void Execute()
    {
        throw new System.NotImplementedException();
    }
}

