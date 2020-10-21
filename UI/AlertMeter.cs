using UnityEngine;

public class AlertMeter : MonoBehaviour
{
    public GameObject[] redSlices;

    private int alert = -1;
    private readonly int maxAlert = 4;

    public void IncreaseAlert()
    {
        if (alert < maxAlert)
        {
            alert++;

            Debug.Log(alert);

            redSlices[alert].SetActive(true);
        }
    }

    public void ReduceAlert()
    {
        if (alert > -1)
        {
            redSlices[alert].SetActive(false);
            alert--;
        }
    }

    public void SetMaxAlert()
    {
        alert = maxAlert;

        for (int i = 0; i <= maxAlert; i++)
        {
            redSlices[i].SetActive(true);
        }
    }
}
