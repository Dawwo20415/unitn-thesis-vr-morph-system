using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class FollowTest : MonoBehaviour
{
    public List<Transform> points;

    private List<Vector3> references;
    private float axisL = 0.1f;
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

    void Start()
    {
        Vector3 midpoint = getMidpoint();

        references = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            references.Add(points[i].position - midpoint);
        }
    }

    void Update()
    {
        Vector3 midpoint = getMidpoint();

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

        Debug.DrawLine(nA, nA + AB, Color.white, Time.deltaTime, false);
        Debug.DrawLine(nA, nA + AC, Color.white, Time.deltaTime, false);
        Debug.DrawLine(nA, nA + N1, Color.white, Time.deltaTime, false);

        Debug.DrawLine(nA, nA + nAB, Color.green, Time.deltaTime, false);
        Debug.DrawLine(nA, nA + nAC, Color.green, Time.deltaTime, false);
        Debug.DrawLine(nA, nA + N2, Color.green, Time.deltaTime, false);

        Vector3 M = Vector3.Cross(N1, N2);

        
        Debug.DrawLine(Vector3.zero, AB, Color.red, Time.deltaTime, false);
        Debug.DrawLine(Vector3.zero, nAB, Color.red, Time.deltaTime, false);
        Debug.DrawLine(Vector3.zero, N1, Color.red, Time.deltaTime, false);

        Quaternion rotation = Quaternion.FromToRotation(N1,N2);

        float angle = Vector3.SignedAngle(rotation * AB, rotation * nAB, N1);

        rotation *= Quaternion.AngleAxis(-angle, N1);

        Debug.DrawLine(nA, nA + (rotation * AB), Color.magenta, Time.deltaTime, false);
        Debug.DrawLine(nA, nA + (rotation * AC), Color.magenta, Time.deltaTime, false);
        Debug.DrawLine(nA, nA + (rotation * N1), Color.magenta, Time.deltaTime, false);

        for (int i = 0; i < points.Count; i++)
        {
            //Debug.DrawLine(midpoint, midpoint + (rotation * references[i]), Color.magenta, Time.deltaTime, false);
            //Debug.DrawLine(midpoint, points[i].position, Color.red, Time.deltaTime, false);
            Debug.DrawLine(midpoint, midpoint + references[i], Color.blue, Time.deltaTime, false);
        }
    }
}
