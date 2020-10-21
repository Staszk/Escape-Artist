using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawnNode : MonoBehaviour
{
    public List<GameObject> spawnableObjects;

    public AudienceInteractable SpawnObject()
    {
        int possibilities = spawnableObjects.Count;

        int chosenIndex = Random.Range(0, possibilities);

        GameObject spawnedObject = Instantiate(spawnableObjects[chosenIndex], transform.position, transform.rotation);

        spawnedObject.transform.SetParent(transform.parent.parent);

        AudienceInteractable audInt = spawnedObject.GetComponent<AudienceInteractable>();

        Destroy(gameObject);

        return audInt;
    }
}
