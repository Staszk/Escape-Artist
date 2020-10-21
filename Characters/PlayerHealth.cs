using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : Health
{
    public static event Action<GameObject, int, bool, bool> ActionDamagePop = delegate { };
    public static event Action ActionPlayerDeath = delegate { };
    public static event Action ActionDamageTaken = delegate { };
    public static event Action<int> ActionTakeDamage = delegate { };

    public GameObject healthObj;
    private GameObject vfx;
    public float screenShakeAmount;

    private IEnumerator HealOverTime(PlayerController p, int healAmount, int healPerTick)
    {
        if (vfx == null)
        {
            vfx = Instantiate(healthObj, transform);
            vfx.transform.localPosition = vfx.transform.localPosition + new Vector3(0.0f, 2f, 0f);
        }

        vfx.SetActive(true);
        int healthLeftToAdd = healAmount;

        while (healthLeftToAdd > 0)
        {
            AddHealth(healPerTick);
            healthLeftToAdd -= healPerTick;
            yield return new WaitForSeconds((healAmount / healPerTick) / 10f);
        }
        vfx.SetActive(false);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        ActionTakeDamage(damage);

        ActionDamageTaken();

        SoundRequest.RequestSound("Punch");

        ActionDamagePop(gameObject, damage, true, false);
    }

    public void HealPlayer(PlayerController p, int healAmount, int healPerTick)
    {
        StartCoroutine(HealOverTime(p, healAmount, healPerTick));
    }

    public void AddHealth(int value)
    {
        currHealth += value;

        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
    }

    protected override void Die()
    {
        // End Game Run
        ActionPlayerDeath();
    }

    protected override void ScreenShake()
    {
        Camera.main.GetComponent<ScreenShake>().Shake(screenShakeAmount);
    }
}
