using UnityEngine;
using System.Collections.Generic;

public class SideObjectiveSpawnNode : MonoBehaviour
{
    public List<GameObject> possibleObjects;
    //public bool isPainting;
    //public Material[] paintings;

    public void SpawnSideObjective()
    {

        int rand = Random.Range(0, possibleObjects.Count);

        GameObject spawnedObject = Instantiate(possibleObjects[rand], transform.position, transform.rotation);

        spawnedObject.transform.SetParent(transform.parent.parent);

        //if(isPainting)
        //{
        //    spawnedObject.transform.GetChild(0).GetComponent<MeshRenderer>().materials[1] = paintings[Random.Range(0, paintings.Length)];
        //}

        Destroy(gameObject);
    }
}
