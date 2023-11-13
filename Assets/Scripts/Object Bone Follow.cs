using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

public class ObjectBoneFollow : MonoBehaviour
{
    [SerializeField]
    private List<Transform> points;
    [SerializeField]
    private Vector3 midpoint_offset;
    [SerializeField]
    private Quaternion rotation_offset;

    private List<Vector3> references;

    void Start()
    {
        if (references.Count < 3)
        {
            Debug.LogError("Not enough reference points", this);
            Debug.Break();
        }
            
        SetReferences(getMidpoint());    
    }
    void Update()
    {
        if (points.Count == 0)
        {
            Debug.LogWarning("Object cannot find points to define bone to follow, is creation of this object set up properly?", this);
            return;
        }

        Vector3 midpoint = getMidpoint();

        Quaternion rot = getRotation(midpoint) * rotation_offset;
        transform.rotation = rot;
        transform.position = midpoint + (rot * midpoint_offset);
        Debug.DrawLine(midpoint, midpoint + (rot * midpoint_offset), UnityEngine.Color.cyan, Time.deltaTime, false);
    }

    public void calibrate(List<Transform> point_list, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        points = point_list;

        Vector3 midpoint = getMidpoint();

        midpoint_offset = position - midpoint;
        rotation_offset = rotation;
        SetReferences(midpoint);

        transform.position = position;
        transform.localScale = scale;
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

        return rotation2 * rotation1;
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
