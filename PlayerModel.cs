using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    //This was needed for the animation event
    private PlayerController pc;

    private void OnEnable()
    {
        pc = transform.parent.GetComponent<PlayerController>();
    }

    public void SwingFinish()
    {
        pc.SwingFinish();
    }

    public void CompleteStep()
    {
        pc.CompleteStep();
    }
}
