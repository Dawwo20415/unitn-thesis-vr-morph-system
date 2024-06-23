using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowNormals : MonoBehaviour
{
    private Mesh m_mesh;
    // Start is called before the first frame update
    void Start()
    {
        m_mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 A = m_mesh.vertices[m_mesh.triangles[0]];
        Vector3 B = m_mesh.vertices[m_mesh.triangles[1]];
        Vector3 C = m_mesh.vertices[m_mesh.triangles[2]];

        Vector3 AB = B - A;
        Vector3 AC = C - A;

        Vector3 N = Vector3.Cross(AB, AC).normalized;

        Vector3 S = (A + B + C) / 3;

        Debug.DrawLine(S, S + N, Color.magenta, Time.deltaTime, false);
    }
}
