using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromToLine : MonoBehaviour
{

    public Transform target;

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, target.position, Color.black, Time.deltaTime, false);
    }
}
