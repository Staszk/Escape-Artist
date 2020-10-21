using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewAngle;
    public float meshResolution;
    public MeshFilter viewMeshFilter;
    public GameObject fovObject;
    private Mesh mesh;
    public bool show;
    public float radius;
    public LayerMask obstacleLayer;

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }

    public class EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 a, Vector3 b)
        {
            pointA = a;
            pointB = b;
        }
    }

    private void OnEnable()
    {
        mesh = new Mesh();
        viewMeshFilter.mesh = mesh;
    }

    public void UpdateView()
    {
        if (show)
        {
            DrawFieldOfView();
        }
    }

    public void DisableFOV()
    {
        show = false;
        fovObject.SetActive(false);
    }

    public void EnableFOV()
    {
        show = true;
        fovObject.SetActive(true);
    }

    private Vector3 DirFromAngle(float degrees)
    {
        return new Vector3(Mathf.Sin(degrees * Mathf.Deg2Rad), 0, Mathf.Cos(degrees * Mathf.Deg2Rad));
    }

    public bool FindVisibleTargets()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                Vector3 dir = (col.gameObject.transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dir) < viewAngle / 2)
                {
                    // return true;
                    int layer = (1 << 10) | (1 << 13) | (1 << 18);
                    float maxDistance = Vector3.Distance(col.gameObject.transform.position, transform.position);
                    Physics.Raycast(transform.position, col.gameObject.transform.position - transform.position, out RaycastHit hit, maxDistance, layer);

                    if (hit.collider != null && hit.collider.tag == "Player")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; ++i)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        //The gameObject's position
        vertices[0] = new Vector3(0,1.1f,0);

        for (int i = 0; i < vertexCount - 1; ++i)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private ViewCastInfo ViewCast(float angle)
    {
        Vector3 dir = DirFromAngle(angle);


        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, radius, obstacleLayer))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, angle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * radius, radius, angle);
        }
    }
}
