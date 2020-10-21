using UnityEngine;
using System.Collections;
using System;

public class EnemyHealth : Health
{
    public static event Action ActionDamageHappened = delegate { };
    public static event Action<int> ActionTakeDamage = delegate { };

    public override void TakeDamage(int damage)
    {
        ActionDamageHappened();
        ActionTakeDamage(damage);

        base.TakeDamage(damage);

        SoundRequest.RequestSound("Hit_Sound");
    }
    protected override void Die()
    {
        GetComponent<Enemy>().SetKnockedOut(true);
    }
}
