using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Test2 : MonoBehaviour
{
    public bool debug = true;

    [SerializeField]
    private List<Transform> points;
    [SerializeField]
    private Vector3 midpoint_offset;
    [SerializeField]
    private Quaternion rotation_offset;

    private List<Vector3> references;

    // 0-1 = Error | 2 = 2 points | 3> = 3 points
    private int mode;

    private void Awake()
    {
        
    }

    private void Update()
    {
        mode = references.Count;
        SetReferences(getMidpoint());
        Vector3 midpoint = getMidpoint();
        midpoint_offset = transform.position - midpoint;
        Debug.DrawLine(midpoint, midpoint + midpoint_offset, Color.cyan, Time.deltaTime, false);
        Quaternion rotation = getRotation(midpoint);
    }

    void SetReferences(Vector3 midpoint)
    {
        references = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            references.Add(points[i].position - midpoint);
        }
    }

    Quaternion getRotation(Vector3 midpoint)
    {
        Quaternion rotation = Quaternion.identity;

        if (mode == 2)
        {
            Vector3 up = points[0].position - points[1].position;
            float c = -(up.x + up.y) / up.z;
            Vector3 fwd = new Vector3(1, 1, c);
            if (debug)
            {
                Debug.DrawLine(points[1].position, points[1].position + up, Color.white, Time.deltaTime, false);
                Debug.DrawLine(midpoint, midpoint + fwd, Color.red, Time.deltaTime, false);
            }
            rotation = Quaternion.LookRotation(fwd, up);

        }
        else if (mode > 2)
        {
            Vector3 A = midpoint + references[0];
            Vector3 B = midpoint + references[1];
            Vector3 C = midpoint + references[2];

            Vector3 nA = points[0].position;
            Vector3 nB = points[1].position;
            Vector3 nC = points[2].position;

            Vector3 AB = B - A;
            Vector3 AC = C - A;

            Vector3 nAB = nB - nA;
            Vector3 nAC = nC - nA;

            Vector3 N1 = Vector3.Cross(AB, AC).normalized;
            Vector3 N2 = Vector3.Cross(nAB, nAC).normalized;

            Quaternion rotation1 = Quaternion.FromToRotation(N1, N2);
            Quaternion rotation2 = Quaternion.FromToRotation(rotation1 * AB, nAB);

            rotation = rotation2 * rotation1;
        }
        return rotation;
    }

    Vector3 getMidpoint()
    {
        Vector3 midpoint = Vector3.zero;

        foreach (Transform point in points)
        {
            midpoint += point.position;
        }

        midpoint /= points.Count;

        return midpoint;
    }
}
