using System;
using UnityEngine;
public class StartButton : MenuButton
{
    public static event Action ActionStartButtonPressed = delegate { };

    public override void PressButton()
    {
        base.PressButton();

        ActionStartButtonPressed();

        FindObjectOfType<AudienceInteractableManager>().SetReadyToPlay();

        SoundRequest.RequestMusic("Game_Music");
    }
}
