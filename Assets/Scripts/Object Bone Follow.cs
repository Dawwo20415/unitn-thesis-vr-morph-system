using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using UnityEngine;

public class ObjectBoneFollow : MonoBehaviour
{
    [SerializeField]
    private List<Transform> points;
    [SerializeField]
    private Vector3 midpoint_offset;
    [SerializeField]
    private Quaternion rotation_offset;

    // Update is called once per frame
    void Update()
    {
        if (points.Count == 0)
        {
            Debug.LogWarning("Object cannot find points to define bone to follow, is creation of this object set up properly?", this);
            return;
        }

        Debug.Log("Rotation Offset: " + rotation_offset.ToString(), this);

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

        transform.position = position;
        transform.rotation = getRotation(midpoint) * rotation_offset;
        transform.localScale = scale;
    }

    Quaternion getRotation(Vector3 midpoint)
    {
        Vector3 pointer = (points[0].position - midpoint).normalized;
        Quaternion rotation = Quaternion.LookRotation(pointer);
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
