using UnityEngine;
using System.Collections;

public class AlertEffectInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Alert Effect", InteractableState.positive, room);

        ID = 9;
    }

    public override void Execute()
    {
        //
    }
}
