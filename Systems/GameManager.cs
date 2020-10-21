using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelGeneration), typeof(RoomManager))]
public class GameManager : MonoBehaviour
{
    private GameRun gameRun;
    private ControllerInputSystem controllerInput; 
    private ScreenUISystem screenUI;

    private void Start()
    {
        // Set up private members
        gameRun = FindObjectOfType<GameRun>();
        controllerInput = GetComponent<ControllerInputSystem>();
        screenUI = FindObjectOfType<ScreenUISystem>().GetComponent<ScreenUISystem>();

        // Create new Run
        gameRun.Initialize();
    }

    private void OnEnable()
    {
        StartButton.ActionStartButtonPressed += StartGame;
        VisualMenuButton.ActionButtonPressed += ShowComic;
        ToMenuButton.ActionButtonPressed += ReturnToMenu;
    }

    private void OnDisable()
    {
        VisualMenuButton.ActionButtonPressed -= ShowComic;
        StartButton.ActionStartButtonPressed -= StartGame;
        ToMenuButton.ActionButtonPressed -= ReturnToMenu;
    }

    private void ShowComic()
    {
        screenUI.SetActivePanel(3);
        controllerInput.ChangeState(GameState.Visual_Menu);
    }

    private void ReturnToMenu()
    {
        screenUI.SetActivePanel(0);
        controllerInput.ChangeState(GameState.Main_Menu);
    }

    private void StartGame()
    {
        screenUI.SetActivePanel(1);
        controllerInput.ChangeState(GameState.Game);
        gameRun.BeginRun();
    }
}
