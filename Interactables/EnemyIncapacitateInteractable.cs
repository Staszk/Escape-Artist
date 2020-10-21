using UnityEngine;
using System.Collections;

public class EnemyIncapacitateInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("EMP Guards " + name, InteractableState.negative, room);

        ID = 2;
    }

    public override void Execute()
    {
        IncapacitateGuards();
    }

    private void IncapacitateGuards()
    {
        SecureRoom room = (SecureRoom)parentRoom;
        room.EMPGuards();
    }
}
