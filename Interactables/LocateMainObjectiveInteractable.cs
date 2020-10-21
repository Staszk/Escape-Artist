using UnityEngine;
using System.Collections;

public class LocateMainObjectiveInteractable : AudienceInteractable
{
    private bool show = false;

    private MainObjectArrow arrow;

    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Guide Player", InteractableState.negative, room);

        ID = 4;
    }

    private void Update()
    {
        if (show)
        {
            LocateMainObjective();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(StayOn());
        }
    }

    public override void Execute()
    {
        StartCoroutine(StayOn());
    }

    private void LocateMainObjective()
    {
        Vector3 playerPos = FindObjectOfType<CameraSmoothFollow>().GetCurrentRoom().transform.position;
        playerPos.y = 0;
        Vector3 location = FindObjectOfType<MainObjectiveRoom>().transform.position;
        location.y = 0;

        Vector3 angleVec = location - playerPos;
        //angleVec.y = 0;

        float angle = Vector3.SignedAngle(angleVec, transform.forward, Vector3.up);

        arrow.RotateMe(angle);
    }

    private IEnumerator StayOn()
    {
        if (arrow == null)
            arrow = FindObjectOfType<GameScreenSystem>().locateObject.GetComponent<MainObjectArrow>();
        arrow.Show(true);
        show = true;

        yield return new WaitForSeconds(2.5f);

        arrow.Show(false);
        show = false;
    }
}
