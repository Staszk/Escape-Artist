using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyFade : MonoBehaviour
{
    private TextMeshProUGUI text;

    public void SetUp()
    {
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text)
    {
        this.text.text = "+" + text;
    }

    public void SetColor(Color32 col)
    {
        text.color = col;
    }

    private void OnEnable()
    {
        StartCoroutine(TurnOff());
    }

    private IEnumerator TurnOff()
    {
        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
    }
}
