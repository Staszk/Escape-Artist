using UnityEngine;
using System.Collections;
using TMPro;

public class MenuButton : MonoBehaviour, IMenuButton
{
    private TextMeshProUGUI buttonText;

    public void Init(bool isActive)
    {
        buttonText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        ButtonActive(isActive);
    }

    public void ButtonActive(bool isActive)
    {
        if (isActive)
        {
            buttonText.color = Color.white;
        }
        else
        {
            buttonText.color = Color.grey;
        }
    }

    public virtual void PressButton()
    {
        SoundRequest.RequestSound("Button");
    }
}

public interface IMenuButton
{
    void PressButton();
}