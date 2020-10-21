using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private float shake;
    public float shakeAmount;
    public float decreaseFactor;

    void Start()
    {
        
    }

    void Update()
    {
        if(shake > 0)
        {
            Camera.main.transform.localPosition += Random.insideUnitSphere * shakeAmount;
            shake -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shake = 0;
        }
    }

    public void Shake(float amount)
    {
        shake = amount;
    }
}
