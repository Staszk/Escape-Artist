using UnityEngine;
using System;

public class CameraInteractable : AudienceInteractable
{
    public static event Action ActionSawPlayer = delegate { };

    public float rotationSpeed;
    [Range(0.025f, 0.2f)]
    public float rotationAngle;
    public GameObject sight;
    private float originalRotation;
    private Animator anim;
    public GameObject particle;
    public Transform rotater;


    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Camera " + name, state, room);

        originalRotation = rotater.rotation.y;

        anim = sight.GetComponent<Animator>();

        if (state == InteractableState.positive)
        {
            sight.GetComponent<CameraSight>().detectPlayer = false;

            sight.SetActive(false);
            particle.SetActive(true);


        }
        else
        {
            sight.GetComponent<CameraSight>().detectPlayer = true;
            particle.SetActive(false);

        }

        ID = 0;
    }

    public override void Execute()
    {
        CameraAction();
    }

    private void CameraAction()
    {
        if(state == InteractableState.negative)
        {
            sight.SetActive(false);
            state = InteractableState.positive;
            sight.GetComponent<CameraSight>().detectPlayer = false;
            particle.SetActive(true);

            SecureRoom room = (SecureRoom)parentRoom;

            room.TurnCamerasOnOff(false);
        }
        else
        {
            sight.SetActive(true);
            state = InteractableState.negative;
            sight.GetComponent<CameraSight>().detectPlayer = true;
            particle.SetActive(false);

            SecureRoom room = (SecureRoom)parentRoom;
            room.TurnCamerasOnOff(true);
        }
    }

    public void SetOnOff(bool on)
    {
        if (on)
        {
            sight.SetActive(true);
            state = InteractableState.negative;
            sight.GetComponent<CameraSight>().detectPlayer = true;
            particle.SetActive(false);
        }
        else
        {
            sight.SetActive(false);
            state = InteractableState.positive;
            sight.GetComponent<CameraSight>().detectPlayer = false;
            particle.SetActive(true);
        }
    }

    private void Update()
    {
        if (state == InteractableState.negative)
        {
            Rotate();
        }
    }

    public void SawPlayer()
    {
        ActionSawPlayer();

        SecureRoom newRoom = (SecureRoom)parentRoom;

        if (!newRoom.hasAlerted)
        {
            newRoom.RoomAlerted();
            newRoom.NotifyEnemiesOfPlayer();
        }

        newRoom.AlertCameras();
    }

    public void SetAlert()
    {
        anim.SetBool("Alerted", true);

        SecureRoom newRoom = (SecureRoom)parentRoom;

        if (!newRoom.hasAlerted)
        {
            newRoom.RoomAlerted();
            newRoom.NotifyEnemiesOfPlayer();
            newRoom.AlertCameras();
        }
    }

    private void Rotate()
    {
        if(rotater.rotation.y > originalRotation + rotationAngle || rotater.rotation.y < originalRotation - rotationAngle)
        {
            rotationSpeed *= -1;
        }

        rotater.Rotate(0, rotationSpeed, 0);
    }
}
