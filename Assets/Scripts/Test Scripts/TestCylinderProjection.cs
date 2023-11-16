using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCylinderProjection : MonoBehaviour
{
    public Transform aT;
    public Transform bT;
    public Transform pT;
    [Range(0.0f,1.0f)]
    public float radius;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;
        Vector3 p = pT.position;

        a = transform.TransformPoint(a);
        b = transform.TransformPoint(b);

        Vector3 AB = b - a;
        Vector3 AP = p - a;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

        Vector3 to_projection = (p - projection_on_line).normalized * radius;

        Debug.DrawLine(a, projection_on_line, Color.green, Time.deltaTime, false);
        Debug.DrawLine(projection_on_line, projection_on_line + to_projection, Color.red, Time.deltaTime, false);
        Debug.DrawLine(p, projection_on_line + to_projection, Color.blue, Time.deltaTime, false);

    }
}
