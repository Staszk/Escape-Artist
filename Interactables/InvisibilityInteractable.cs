using UnityEngine;
using System.Collections;

public class InvisibilityInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Invisibility", InteractableState.negative, room);

        ID = 8;
    }

    public override void Execute()
    {
        //
    }
}
