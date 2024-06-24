using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ConnectPointsEditor : MonoBehaviour
{
    public Color col;
    public Transform trn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, trn.position, col, Time.deltaTime, false);
    }
}
