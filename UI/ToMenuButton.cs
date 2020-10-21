using UnityEngine;
using System.Collections;
using System;

public class ToMenuButton : MenuButton
{
    public static event Action ActionButtonPressed = delegate { };

    public override void PressButton()
    {
        base.PressButton();

        ActionButtonPressed();
    }

}
