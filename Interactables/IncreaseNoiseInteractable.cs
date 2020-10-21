using UnityEngine;

public class IncreaseNoiseInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Noisy Player", InteractableState.positive, room);

        ID = 10;
    }

    public override void Execute()
    {
        //
    }
}
