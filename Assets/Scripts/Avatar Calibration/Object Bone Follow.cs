using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectBoneFollow : MonoBehaviour
{
    public bool debug = false;

    [SerializeField]
    private List<Transform> points;
    [SerializeField]
    private Vector3 midpoint_offset;
    public Quaternion rotation_offset;

    private List<Vector3> references;
    
    // 0-1 = Error | 2 = 2 points | 3> = 3 points
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
    private Quaternion m_rotation;
    [SerializeField]
    private Quaternion m_globalRotation;

    void Start()
    {
         
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
        m_globalRotation = getRotation(midpoint);
        transform.rotation = rot;
        transform.position = midpoint - (rot * midpoint_offset);
        if (debug)
        {
            Debug.DrawLine(midpoint, midpoint + (rot * Vector3.forward), Color.cyan, Time.deltaTime, false);
            Debug.DrawLine(midpoint, midpoint - (rot * midpoint_offset), Color.cyan, Time.deltaTime, false);
        }
    }

    public void calibrate(List<Transform> point_list, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        points = point_list;

        transform.localPosition = position;
        transform.localScale = scale;

        Vector3 midpoint = getMidpoint();
        midpoint_offset = midpoint - transform.position;
        rotation_offset = QExtension.Fix(rotation);
        SetReferences(midpoint);

        transform.localRotation = getRotation(midpoint) * rotation_offset;

        if (references.Count < 1)
        {
            mode = Mode.Error;
            Debug.LogError("Not enough reference points", this);
            Debug.Break();
        }

        if (references.Count == 1) { mode = Mode.OnePoint; }
        else if (references.Count == 2) { mode = Mode.TwoPoints; }
        else { mode = Mode.ThreePoints; }
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
        
        if (mode == Mode.OnePoint)
        {
            rotation = points[0].rotation;
        }
        else if (mode == Mode.TwoPoints)
        {
            rotation = points[0].rotation;

        } else if (mode == Mode.ThreePoints)
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
        m_rotation = rotation;
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
