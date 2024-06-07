using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DebugDisplayDirections : MonoBehaviour
{
    public float length = 1.0f;
    public bool stdColor = true;
    public Color col;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.forward * length), stdColor ? Color.blue  : col, Time.deltaTime, false);
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.right   * length), stdColor ? Color.red   : col, Time.deltaTime, false);
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.up      * length), stdColor ? Color.green : col, Time.deltaTime, false);
    }
}
