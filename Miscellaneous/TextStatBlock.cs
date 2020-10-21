using UnityEngine;
using System.Collections;
using TMPro;

public class TextStatBlock : MonoBehaviour
{
    public TMP_Text statText;

    public void SetText(string text)
    {
        statText.text = text;
    }
}
