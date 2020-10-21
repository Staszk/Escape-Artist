using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    private void DestroyThis()
    {
        GameObject temp = transform.parent.gameObject;
        transform.parent = null;
        Destroy(temp);
        Destroy(gameObject);
    }
}
