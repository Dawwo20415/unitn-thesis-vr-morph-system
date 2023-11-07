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

        transform.position = midpoint + midpoint_offset;
        transform.rotation = getRotation(midpoint) * rotation_offset;
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
            references.Add(midpoint - points[i].position);
        }
    }

    Quaternion getRotation(Vector3 midpoint)
    {
        Vector3 average = Vector3.zero;
        float weight = 0.0f;
        float angle = 0.0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pos = midpoint - points[i].position;
            Vector3 cross = Vector3.Cross(pos, references[i]);

            average += cross * cross.magnitude;
            weight += cross.magnitude;

            angle += Mathf.Asin(cross.magnitude / (pos.magnitude * references[i].magnitude));
        }

        average /= weight;
        angle /= points.Count;
        return Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, average);
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
