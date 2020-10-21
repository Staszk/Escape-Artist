using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider healthSlider;

    public void Initialize()
    {
        healthSlider = transform.GetChild(0).GetComponent<Slider>();
    }

    public void SetValue(int value)
    {
        healthSlider.value = value;
    }

    public void SetMax(int max)
    {
        healthSlider.maxValue = max;
    }
}
