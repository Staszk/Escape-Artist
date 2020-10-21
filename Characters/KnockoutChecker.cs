using UnityEngine;
using System;

public class KnockoutChecker : MonoBehaviour
{
    public static event Action<GameObject, bool> ActionEnemyInRange = delegate { };
    public static event Action ActionEnemyKnockedOut = delegate { };

    private Enemy enemy;
    public PlayerController player;
    public void Initialize(PlayerController player)
    {
        //this.player = player;
    }

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void Knockout()
    {
        if (enemy != null)
        {
            if (!enemy.GetKnockedOut() && !enemy.GetNoticedPlayer())
            {
                //Raycast on the Obstacle layer to see if there is a wall in the way
                int layer = (1 << 13) | (1 << 18);
                float maxDistance = Vector3.Distance(enemy.transform.position, transform.position);
                Physics.Raycast(transform.position, enemy.transform.position - transform.position, out RaycastHit hit, maxDistance, layer);

                //If the raycast didn't hit anything, then we can do the knockout
                if (hit.collider == null)
                {
                    enemy.SetKnockedOut(true);

                    ActionEnemyKnockedOut();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attackable"))
        {
            Enemy enemyScript = other.GetComponent<Enemy>();
            if (!enemyScript.GetNoticedPlayer())
            {
                enemy = enemyScript;
                ActionEnemyInRange(enemy.gameObject, true);
                enemy.PlayerClose();
                player.SetKnockout(true);
            }
            else
            {
                player.SetKnockout(false);
            }
        }
        else
        {
            player.SetKnockout(false);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemy != null && other.gameObject.GetComponent<Enemy>() == enemy)
        {
            ActionEnemyInRange(enemy.gameObject, false);
            enemy.PlayerFar();
            enemy = null;
        }
    }
}