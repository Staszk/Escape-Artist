using UnityEngine;
using System.Collections;
using System;

public class Health : MonoBehaviour
{
    public static event Action ActionEnemyKnockedOut = delegate { };

    public int maxHealth;
    protected int currHealth;
    public Animator anim;

    public void Init()
    {
        currHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        currHealth -= damage;

        if (currHealth > 0)
        {
            if (anim != null)
            {
                anim.SetBool("TakeDamage", true);
                StartCoroutine(EndDamageAnimation());
            }
            ScreenShake();
        }
        else
        {
            if (this is EnemyHealth)
            {
                ActionEnemyKnockedOut();
            }

            Die();
        }
    }

    protected virtual void Die()
    {
        // Empty
    }

    protected virtual void ScreenShake()
    {
        // Empty
    }

    private IEnumerator EndDamageAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("TakeDamage", false);
    }

    public int GetCurrHealth()
    {
        return currHealth;
    }

    public void ZeroOutHealth()
    {
        currHealth = 0;

        // Added this, possibly fixes stealth takedown
        Die();
    }

    public void ResetHealth()
    {
        currHealth = maxHealth;
    }
       
}
