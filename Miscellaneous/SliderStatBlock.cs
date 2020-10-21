using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SliderStatBlock : MonoBehaviour
{
    public Image goodTeam;
    public Image badTeam;
    public Image cutOff;

    public TMP_Text goodText;
    public TMP_Text badText;

    private readonly float localXMax = 100f;
    private readonly float localXMin = -100f;

    public void SetUp(float goodPercent, float badPercent, int goodNum, int badNum)
    {
        float distance = goodPercent;

        goodTeam.fillAmount = goodPercent;

        badTeam.fillAmount = badPercent;

        cutOff.rectTransform.localPosition = new Vector2(Mathf.Lerp(localXMin, localXMax, distance), 0);

        goodText.text = goodNum.ToString();
        badText.text = badNum.ToString();
    }
}
