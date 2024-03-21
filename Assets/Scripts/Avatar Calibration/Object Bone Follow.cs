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
    [SerializeField]
    private Quaternion rotation_offset;

    private List<Vector3> references;
    
    // 0-1 = Error | 2 = 2 points | 3> = 3 points
    private int mode;

    void Start()
    {
        SetReferences(getMidpoint());

        if (references.Count < 2)
        {
            Debug.LogError("Not enough reference points", this);
            Debug.Break();
        }

        mode = references.Count;  
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
        if (debug)
        {
            Debug.DrawLine(midpoint, midpoint + (rot * midpoint_offset), Color.cyan, Time.deltaTime, false);
        }
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
        transform.rotation = getRotation(midpoint) * rotation_offset;
    }

    [ContextMenu("Calibrate Statically")]
    public void staticCalibration()
    {
        Vector3 midpoint = getMidpoint();
        SetReferences(midpoint);
        midpoint_offset = transform.position - midpoint;
        rotation_offset = getRotation(midpoint) * transform.rotation;
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
            Vector3 fwd = new Vector3(1,1, c);
            if (debug)
            {
                Debug.DrawLine(points[1].position, points[1].position + up, Color.white, Time.deltaTime, false);
                Debug.DrawLine(midpoint, midpoint + fwd, Color.red, Time.deltaTime, false);
            }
            rotation = Quaternion.LookRotation(fwd, up);

        } else if (mode > 2)
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
