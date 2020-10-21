using UnityEngine;
using System.Collections;

public class TripwireLaser : MonoBehaviour
{
    private TripwireInteractable parent;

    public void Initialize(TripwireInteractable parent)
    {
        this.parent = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parent.Tripped();

            //Do a coroutine cause we want the player to actually be in the tripwire when they get stunned instead of slightly touching it
            StartCoroutine(StunPlayer(other));
        }
    }

    private IEnumerator StunPlayer(Collider other)
    {
        yield return new WaitForSeconds(0.0f);
        other.GetComponent<PlayerController>().Stun();
    }
}
