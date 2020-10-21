using UnityEngine;
using System.Collections;
using InControl;

public class VisualMenu : MonoBehaviour
{
    private InputDevice controller;

    public MenuButton button;

    private void OnEnable()
    {
        ControllerInputSystem.ActionVisualMenuInput += GetControls;
    }

    private void OnDisable()
    {
        ControllerInputSystem.ActionVisualMenuInput -= GetControls;
    }

    private void Update()
    {
        if (controller.Action1.WasPressed)
        {
            button.PressButton();
        }

        controller = null;
    }

    private void GetControls(InputDevice device)
    {
        controller = device;
    }
}
