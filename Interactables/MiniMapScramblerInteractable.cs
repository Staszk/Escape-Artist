using UnityEngine;

public class MiniMapScramblerInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Scramble Map", InteractableState.positive, room);

        ID = 5;
    }


    public override void Execute()
    {
        MiniMapScrambleAction();
    }

    private void MiniMapScrambleAction()
    {
        Debug.Log("MiniMapScrambled");
        //scrambling the minimap
        gameObject.SetActive(true);
        //state = InteractableState.negative;
    }
}
