using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


public class PieChart : MonoBehaviour
{
    public Image[] wedges;

    public Image icon;
    public TMP_Text playStyleText;
    public TMP_Text descriptionText;

    [TextArea(1, 2)]
    public string[] playStyles;

    [TextArea(1,2)]
    public string[] descriptions;

    public Sprite[] icons;

    public void SetUp(float[] percents, int style)
    {
        float zRot = 0f;

        for (int i = 0; i < wedges.Length; i++)
        {
            wedges[i].fillAmount = percents[i];
            wedges[i].rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, zRot));
            zRot -= wedges[i].fillAmount * 360f;
        }

        playStyleText.text = playStyles[style];
        descriptionText.text = descriptions[style];
        icon.sprite = icons[style];
    }
}
