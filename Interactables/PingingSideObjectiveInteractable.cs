using UnityEngine;

public class PingingSideObjectiveInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Locate Art", InteractableState.negative, room);

        ID = 6;
    }

    public override void Execute()
    {
        PingObjectiveAction();
    }

    private void PingObjectiveAction()
    {
        Debug.Log("Side Objective Pinged");
        //scrambling the minimap
        gameObject.SetActive(true);
        //state = InteractableState.negative;
    }
}
