using UnityEngine;
using System;

public class VisualMenuButton : MenuButton
{
    public static event Action ActionButtonPressed = delegate { };

    public override void PressButton()
    {
        base.PressButton();

        ActionButtonPressed();
    }
}
