using UnityEngine;
using System.Collections;

public class QuitButton : MenuButton
{
    public override void PressButton()
    {
        base.PressButton();

        Application.Quit();
    }
}
