using UnityEngine;
using InControl;

public class MainMenuSystem : MonoBehaviour
{
    private MenuButton[] buttons;
    private int currentSelection;

    private InputDevice controller;

    private void Start()
    {
        currentSelection = 0;

        buttons = new MenuButton[3];
        buttons[0] = transform.GetChild(0).GetChild(0).GetComponent<MenuButton>();
        buttons[1] = transform.GetChild(0).GetChild(1).GetComponent<MenuButton>();
        buttons[2] = transform.GetChild(0).GetChild(2).GetComponent<MenuButton>();

        buttons[0].Init(true);
        buttons[1].Init(false);
        buttons[2].Init(false);

        SoundRequest.RequestMusic("Menu_Music");
    }

    private void OnEnable()
    {
        ControllerInputSystem.ActionMainMenuInput += UpdateController;
    }

    private void OnDisable()
    {
        ControllerInputSystem.ActionMainMenuInput -= UpdateController;

    }

    private void Update()
    {
        if (controller != null)
        {
            if (controller.DPadDown.WasPressed || controller.LeftStickDown.WasPressed)
            {
                buttons[currentSelection].ButtonActive(false);

                ++currentSelection;

                if (currentSelection > 2)
                {
                    currentSelection = 0;
                }

                SoundRequest.RequestSound("Menu_Browse");

                buttons[currentSelection].ButtonActive(true);
            }
            else if (controller.DPadUp.WasPressed || controller.LeftStickUp.WasPressed)
            {
                buttons[currentSelection].ButtonActive(false);


                --currentSelection;

                if (currentSelection < 0)
                {
                    currentSelection = 2;
                }

                SoundRequest.RequestSound("Menu_Browse");

                buttons[currentSelection].ButtonActive(true);
            }
            else if (controller.Action1.WasPressed)
            {
                buttons[currentSelection].PressButton();
            }
        }

        controller = null;
    }

    private void UpdateController(InputDevice device)
    {
        controller = device;
    }
}
