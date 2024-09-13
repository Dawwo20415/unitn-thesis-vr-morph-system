using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyMeshBoneWeights : MonoBehaviour
{
    private enum Mode
    {
        Error,
        OnePoint,
        TwoPoints,
        ThreePoints
    }
    [SerializeField]
    private Mode mode;

    [SerializeField]
    private List<Transform> points;
    private List<Vector3> references;

    [SerializeField] private Vector3 midpoint_offset;
    public Quaternion m_rotation_offset;

    public void Calibrate(List<Transform> point_list, Vector3 position_offset, Quaternion rotation_offset)
    {
        points = point_list;

        midpoint_offset = position_offset;
        m_rotation_offset = Quaternion.identity;

        transform.position = getMidpoint() - position_offset;
        transform.rotation = rotation_offset;

        if (points.Count == 1) { mode = Mode.OnePoint; }
        else if (points.Count == 2) { mode = Mode.TwoPoints; }
        else { mode = Mode.ThreePoints; }
    }

    // Update is called once per frame
    void Update()
    {
        if (points.Count == 0) { Debug.LogWarning("Object cannot find points to define bone to follow, is creation of this object set up properly?", this); this.enabled = false; }

        Vector3 midpoint = getMidpoint();
        Quaternion rot = getRotation(midpoint) * Quaternion.Inverse(m_rotation_offset);

        transform.rotation = rot;
        transform.position = midpoint - (rot * midpoint_offset);


    }

    private Quaternion getRotation(Vector3 midpoint)
    {
        Quaternion rotation = Quaternion.identity;

        if (mode == Mode.OnePoint || mode == Mode.TwoPoints)
        {
            rotation = points[0].rotation;
        }
        /*else if (mode == Mode.ThreePoints)
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
        }*/

        return rotation;
    }

    private Vector3 getMidpoint()
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
