using UnityEngine;
using System.Collections;
using InControl;
using System;

public enum GameState
{
    None,
    Main_Menu,
    Visual_Menu,
    Game,
    Pause_Menu,
    End_Screen
}

public class ControllerInputSystem : MonoBehaviour
{
    public static event Action<InputDevice> ActionMainMenuInput = delegate { };
    public static event Action<InputDevice> ActionGameInput = delegate { };
    public static event Action<InputDevice> ActionPauseMenuInput = delegate { };
    public static event Action<InputDevice> ActionEndScreenInput = delegate { };
    public static event Action<InputDevice> ActionVisualMenuInput = delegate { };
    public static event Action<GameState> ActionGameStateChange = delegate { };

    private GameState currentState;
    private InputDevice[] controllers;

    private void Start()
    {
        controllers = new InputDevice[3];

        ChangeState(GameState.Main_Menu);

        StartCoroutine(SetupInput());
    }

    private void Update()
    {
        if (controllers[0] != null)
        {
            SendOutInputDevice();
        }
    }

    private void SendOutInputDevice()
    {
        switch (currentState)
        {
            case GameState.None:
                break;
            case GameState.Main_Menu:
                {
                    ActionMainMenuInput(controllers[0]);
                }
                break;
            case GameState.Visual_Menu:
                {
                    ActionVisualMenuInput(controllers[0]);
                }
                break;
            case GameState.Game:
                {
                    ActionGameInput(controllers[0]);
                }
                break;
            case GameState.Pause_Menu:
                {
                    ActionPauseMenuInput(controllers[0]);
                }
                break;
            case GameState.End_Screen:
                {
                    ActionEndScreenInput(controllers[0]);
                }
                break;
            default:
                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            ActionGameStateChange(currentState);
        }
    }

    private IEnumerator SetupInput()
    {
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < InputManager.Devices.Count; ++i)
        {
            controllers[i] = InputManager.Devices[i];
        }
    }
}
