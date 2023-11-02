using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCylinderProjection : MonoBehaviour
{
    public Transform aT;
    public Transform bT;
    public Transform pT;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 a = aT.position;
        Vector3 b = bT.position;
        Vector3 p = pT.position;

        Vector3 AB = a - b;
        Vector3 AP = a - p;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Debug.DrawLine(a, a + (AB.normalized * ABAPdot), Color.green, Time.deltaTime, false);
        Debug.DrawLine(p, a + (AB.normalized * ABAPdot), Color.red, Time.deltaTime, false);

    }
}
