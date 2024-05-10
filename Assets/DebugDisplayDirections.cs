using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplayDirections : MonoBehaviour
{
    public float length = 1.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.forward * length), Color.blue, Time.deltaTime, false);
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.right * length), Color.red, Time.deltaTime, false);
        Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.up * length), Color.green, Time.deltaTime, false);
    }
}
