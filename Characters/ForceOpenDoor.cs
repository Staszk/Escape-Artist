using System;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class ForceOpenDoor : MonoBehaviour
{
    public static event Action ActionDoorForcedOpen = delegate { };
    public static event Action<GameObject> ActionAllocateSpamButton = delegate { };
    public static event Action<GameObject> ActionUpdateSpamButton = delegate { };
    public static event Action ActionReleaseSpamButton = delegate { };

    private int buttonPresses;
    public int maxButtonPresses;
    private DoorInteractable thisDoor;
    private bool canSpam;
    private float timeWhenPressed;
    private Camera mainCam;

    private InputDevice device;

    private void Start()
    {
        buttonPresses = 0;
        canSpam = false;
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        ControllerInputSystem.ActionGameInput += GetControls;
    }

    private void OnDisable()
    {
        ControllerInputSystem.ActionGameInput -= GetControls;
    }

    private void Update()
    {
        if (canSpam && thisDoor != null && !thisDoor.cannotBeOpened)
        {
            CheckButton();

            ActionUpdateSpamButton(thisDoor.gameObject);
        }

        device = null;
    }

    private void CheckButton()
    {
        if (device != null && device.Action1.WasPressed)
        {
            timeWhenPressed = Time.time;
            buttonPresses++;
            thisDoor.transform.GetChild(0).GetComponent<Animator>().SetBool("Prying", true);
            GetComponentInParent<PlayerController>().SetDoorPry(true);
            if (buttonPresses >= maxButtonPresses && thisDoor.state == InteractableState.negative)
            {
            	thisDoor.transform.GetChild(0).GetComponent<Animator>().SetBool("Prying", false);
                GetComponentInParent<PlayerController>().SetDoorPrySuccess(true);
                thisDoor.Execute();
                ActionReleaseSpamButton();
                ActionDoorForcedOpen();
            }
        }
        else
        {
            if (Time.time - timeWhenPressed > 0.5)
            {
                GetComponentInParent<PlayerController>().SetDoorPrySuccess(false);
                GetComponentInParent<PlayerController>().SetDoorPry(false);
                thisDoor.transform.GetChild(0).GetComponent<Animator>().SetBool("Prying", false);
            }
        }
    }

    private void GetControls(InputDevice device)
    {
        this.device = device;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Door"))
        {
            if(other.transform.parent.GetComponent<DoorInteractable>().state == InteractableState.negative)
            {
                thisDoor = other.transform.parent.GetComponent<DoorInteractable>();
                canSpam = true;

                if (!thisDoor.cannotBeOpened)
                    ActionAllocateSpamButton(thisDoor.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            buttonPresses = 0;

            thisDoor = null;
            canSpam = false;
            GetComponentInParent<PlayerController>().SetDoorPry(false);
            ActionReleaseSpamButton();
        }
    }
}
