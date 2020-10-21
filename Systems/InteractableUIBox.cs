using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class InteractableUIBox : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI interactableName;
    public Image image;

    private readonly float sliderMax = 1.0f;

    private void Start()
    {
        slider.maxValue = sliderMax;
    }

    public void UpdateBox(float value, string name, Sprite spr)
    {
        slider.value = value;
        interactableName.text = name;
        image.sprite = spr;

        gameObject.SetActive(true);
    }

    public void UpdateBox(float value)
    {
        slider.value = value;
    }

    public void UpdateBox(bool show)
    {
        gameObject.SetActive(show);
    }
}
