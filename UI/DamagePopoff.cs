using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopoff : MonoBehaviour
{
    private Color enemyHit = Color.white;
    public Color enemyCrit;
    public Color playerHit;

    public TextMeshProUGUI text;

    public Animator anim;

    public void PopOff(int hitType, string damage)
    {
        text.text = damage;

        switch (hitType)
        {
            case 0:
                text.color = enemyHit;
                break;
            case 1:
                text.color = enemyCrit;
                break;
            case 2:
                text.color = playerHit;
                break;
            default:
                break;
        }

        int rand = Random.Range(0, 3);

        anim.SetInteger("Selection", rand);
    }

    private void OnEnable()
    {
        StartCoroutine(TurnOff());
    }

    private IEnumerator TurnOff()
    {
        yield return new WaitForSeconds(0.75f);

        gameObject.SetActive(false);
    }
}
