using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public float radius;
    public LayerMask attackableLayers;
    public float knockbackForce;

    [Range(0.5f, 0.75f)]
    public float attackAngle = .75f;

    Vector3 edge;

    public GameObject DoAttack(int damage)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, attackableLayers);

        foreach (Collider col in colliders)
        {
            Vector3 norm = Vector3.Normalize(col.transform.position - transform.position);


            if (Vector3.Dot(norm, transform.forward) > attackAngle)
            {
                if (col.CompareTag("Attackable") || col.CompareTag("Player"))
                {
                    col.GetComponent<Health>().TakeDamage(damage);
                    col.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * knockbackForce);

                    if (col.CompareTag("Attackable"))
                    {
                        col.gameObject.GetComponent<Enemy>().SetWasAttacked(true);
                        col.gameObject.GetComponent<Enemy>().Stun();
                    }

                    return col.gameObject;
                }
            }
        }

        return null;
    }

    //private void OnDrawGizmos()
    //{
    //    edge = transform.position - transform.position * radius;
    //    edge = Quaternion.Euler(0, attackAngle / 2, 0) * edge;

    //    Gizmos.DrawWireSphere(transform.position, radius);
    //}
}
