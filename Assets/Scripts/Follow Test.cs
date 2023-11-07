using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTest : MonoBehaviour
{
    public List<Transform> points;

    private List<Vector3> references;
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
            references.Add(midpoint - points[i].position);
        }
    }

    void Update()
    {
        Vector3 midpoint = getMidpoint();

        transform.position = midpoint;

        Vector3 weighted_average = Vector3.zero;
        float weight = 0.0f;

        float average_dot = 0.0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 cross = Vector3.Cross(midpoint - points[i].position, references[i]);
            weighted_average += cross * cross.magnitude;
            weight += cross.magnitude;

            average_dot += Mathf.Asin(cross.magnitude / ((midpoint - points[i].position).magnitude * references[i].magnitude));

            //Debug.DrawLine(midpoint, points[i].position, Color.red, Time.deltaTime, false);
            //Debug.DrawLine(midpoint, midpoint + references[i], Color.blue, Time.deltaTime, false);
            //Debug.DrawLine(midpoint, midpoint + cross, Color.green, Time.deltaTime, false);
        }

        weighted_average /= weight;
        //Debug.DrawLine(midpoint, midpoint + weighted_average.normalized, Color.white, Time.deltaTime, false);

        average_dot /= points.Count;
        transform.rotation = Quaternion.AngleAxis(-average_dot * Mathf.Rad2Deg, weighted_average);    
    }
}
