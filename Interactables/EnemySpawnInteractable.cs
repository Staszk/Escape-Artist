using UnityEngine;

public class EnemySpawnInteractable : AudienceInteractable
{
    public override void InitializeInteractable(string name, InteractableState state, Room room)
    {
        base.InitializeInteractable("Spawn Guard " + name, InteractableState.positive, room);

        ID = 3;
    }

    public override void Execute()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        SecureRoom room = (SecureRoom)parentRoom;
        room.SpawnGuard();
    }
}