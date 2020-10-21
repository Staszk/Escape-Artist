using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tripwire : MonoBehaviour
{
    public GameObject northernTransform;
    public GameObject southernTransform;
    public bool move;
    public float moveSpeed;
    public bool goSouth;
    public bool movesHoriz;

    private void Update()
    {
        if (move)
        {
            if (!movesHoriz)
            {
                if (goSouth)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - moveSpeed * Time.deltaTime);

                    if (transform.position.z < southernTransform.transform.position.z)
                    {
                        goSouth = false;
                    }
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + moveSpeed * Time.deltaTime);

                    if (transform.position.z > northernTransform.transform.position.z)
                    {
                        goSouth = true;
                    }
                }
            }
            else
            {
                if (goSouth)
                {
                    transform.position = new Vector3(transform.position.x - moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);

                    if (transform.position.x < southernTransform.transform.position.x)
                    {
                        goSouth = false;
                    }
                }
                else
                {
                    transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);

                    if (transform.position.x > northernTransform.transform.position.x)
                    {
                        goSouth = true;
                    }
                }
            }
        }
    }
}
