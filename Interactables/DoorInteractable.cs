using UnityEngine;
using System.Collections;
using System;

public class DoorInteractable : AudienceInteractable
{
    public static event Action<DoorInteractable> ActionAllocateDoorLock = delegate { };
    public static event Action<DoorInteractable, int, float> ActionUpdateDoorLock = delegate { };
    public static event Action<DoorInteractable, int> ActionReleaseDoorLock = delegate { };

    private Animator anim;

    public bool cannotBeOpened = false;

    public int referenceID = -1;

    private readonly float maxTime = 30.0f;

    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Door " + name, state, room);

        anim = transform.GetChild(0).GetComponent<Animator>();

        if (state == InteractableState.negative)
        {
            anim.SetBool("Open", false);
            state = InteractableState.negative;
        }
        else
        {
           anim.SetBool("Open", true);
            state = InteractableState.negative;
        }

        ID = 1;
    }

    public override void Execute()
    {
        DoorAction();
    }

    private void DoorAction()
    {
        if(state == InteractableState.negative)
        {
            anim.SetBool("Open", true);
            state = InteractableState.positive;
            SoundRequest.RequestSound("Open");

            StopAllCoroutines();

            if (referenceID != -1)
            {
                ActionReleaseDoorLock(this, referenceID);
            }
        }
        else
        {
            anim.SetBool("Open", false);
            state = InteractableState.negative;
            SoundRequest.RequestSound("Close");

            cannotBeOpened = true;

            StartCoroutine(Unlock());
        }
    }

    private IEnumerator Unlock()
    {
        float timeTillUnlock = maxTime;

        ActionAllocateDoorLock(this);

        while (timeTillUnlock > 0)
        {
            timeTillUnlock -= Time.deltaTime;

            float percentLeft = timeTillUnlock / maxTime;

            ActionUpdateDoorLock(this, referenceID, percentLeft);

            yield return null;
        }

        ActionReleaseDoorLock(this, referenceID);

        cannotBeOpened = false;
    }
}
