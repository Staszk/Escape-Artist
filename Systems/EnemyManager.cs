using UnityEngine;
using System.Collections.Generic;

public class EnemyManager
{
    private List<Enemy> enemyList;

    private Transform playerTransform;

    private readonly float rangeCutOff = 42f;

    public EnemyManager()
    {
        enemyList = new List<Enemy>();
    }

    public void TrackEnemy(Enemy newEnemy)
    {
        enemyList.Add(newEnemy);
    }

    public void TrackPlayer(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    public void UpdateEnemies()
    {
        foreach (Enemy enemy in enemyList)
        {
            if (CompareDistance(enemy.transform.position) <= rangeCutOff)
            {
                enemy.ExecuteFrame();
            }
        }
    }

    public void DeleteEnemies()
    {
        foreach (Enemy e in enemyList)
        {
            e.DestroySelf();
        }
    }

    public void AlertEnemiesLockdown()
    {
        foreach (Enemy e in enemyList)
        {
            e.inLockDown = true;
        }
    }

    private float CompareDistance(Vector3 enemyPosition)
    {
        return Vector3.Distance(enemyPosition, playerTransform.position);
    }
}
