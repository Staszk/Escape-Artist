using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSight : MonoBehaviour
{
    public CameraInteractable cam;
    private Animator anim;
    public bool detectPlayer;


    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void SawThePlayer()
    {
        cam.SawPlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (detectPlayer && other.CompareTag("Player"))
        {
            anim.SetBool("See", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (detectPlayer && other.CompareTag("Player"))
        {
            anim.SetBool("See", false);
        }
    }
}
