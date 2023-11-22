using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromToLine : MonoBehaviour
{

    public Transform target;
    private Color col;

    private void Start()
    {
        col = Random.ColorHSV();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, target.position, col, Time.deltaTime, false);
    }
}
