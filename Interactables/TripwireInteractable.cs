using UnityEngine;
using System;

public class TripwireInteractable : AudienceInteractable
{
    public static event Action ActionLineCrossed = delegate { };
    public TripwireLaser[] tripwireLasers;
    public GameObject p1;
    public GameObject p2;
    public Tripwire mover;
    private bool isMovingTripwire;

    public bool mustStartOn;

    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        isMovingTripwire = mover.move;

        base.InitializeInteractable("Laser Grid " + name, state, room);

        if (mustStartOn)
            state = InteractableState.negative;

        if (state == InteractableState.positive)
        {
            for (int j = 0; j < tripwireLasers.Length; ++j)
            {
                tripwireLasers[j].gameObject.SetActive(false);
            }

            mover.move = false;

            p1.SetActive(true);
            p2.SetActive(true);
        }
        else
        {
            if (isMovingTripwire)
            {
                mover.move = true;
            }

            p1.SetActive(false);
            p2.SetActive(false);
        }

        int i = 0;
        foreach (TripwireLaser child in tripwireLasers)
        {
            TripwireLaser laser = tripwireLasers[i];

            if (laser)
            {
                laser.Initialize(this);
            }
            i++;
        }

        ID = 7;
    }

    public override void Execute()
    {
        TripwireAction();
    }

    private void TripwireAction()
    {
        if(state == InteractableState.negative)
        {
            for (int j = 0; j < tripwireLasers.Length; ++j)
            {
                tripwireLasers[j].gameObject.SetActive(false);
            }
            p1.SetActive(true);
            p2.SetActive(true);
            state = InteractableState.positive;

            mover.move = false;

            SecureRoom room = (SecureRoom)parentRoom;
            room.TurnTripwiresOnOff(false);
        }
        else
        {
            for (int j = 0; j < tripwireLasers.Length; ++j)
            {
                tripwireLasers[j].gameObject.SetActive(true);
            }
            p1.SetActive(false);
            p2.SetActive(false);
            state = InteractableState.negative;

            if (isMovingTripwire)
            {
                mover.move = true;
            }

            SecureRoom room = (SecureRoom)parentRoom;
            room.TurnTripwiresOnOff(true);
        }
    }

    public void SetOnOff(bool on)
    {
        if (on)
        {
            for (int j = 0; j < tripwireLasers.Length; ++j)
            {
                tripwireLasers[j].gameObject.SetActive(true);
            }
            p1.SetActive(true);
            p2.SetActive(true);

            mover.move = false;

            state = InteractableState.negative;
        }
        else
        {
            for (int j = 0; j < tripwireLasers.Length; ++j)
            {
                tripwireLasers[j].gameObject.SetActive(false);
            }
            p1.SetActive(false);
            p2.SetActive(false);

            if (isMovingTripwire)
            {
                mover.move = true;
            }

            state = InteractableState.positive;
        }
    }

    public void Tripped()
    {
        ActionLineCrossed();
        SecureRoom room = (SecureRoom)parentRoom;
        SoundRequest.RequestSound("Zap");

        //if (!room.hasAlerted)
        //{
        //    room.NotifyEnemiesOfPlayer();
        //}
    }
}
