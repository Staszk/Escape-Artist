using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainObjectArrow : MonoBehaviour
{
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    public void RotateMe(float value)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, value));
    }
}
