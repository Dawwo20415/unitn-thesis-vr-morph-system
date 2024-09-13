using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawMesh : MonoBehaviour
{
    private Mesh m_Mesh;
    private List<Transform> m_Anchors;

    public void Setup(BSACustomMesh bsa_mesh, List<Transform> anchors)
    {
        m_Mesh = new Mesh();
        m_Mesh.vertices = bsa_mesh.vertices;
        m_Mesh.triangles = bsa_mesh.triangles;

        m_Mesh.RecalculateNormals();

        m_Anchors = anchors;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (m_Mesh) { Gizmos.DrawWireMesh(m_Mesh, transform.position, transform.rotation); }
    }

    private Vector3 CalcPositionOffset()
    {
        Vector3 midpoint = Vector3.zero;

        foreach (Transform trn in m_Anchors)
        {
            midpoint += trn.position;
        }

        midpoint /= m_Anchors.Count;

        return midpoint - transform.position;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.92f, 0.016f, 0.4f);
        Gizmos.DrawMesh(m_Mesh, transform.position, transform.rotation);
        Gizmos.color = Color.red;
        foreach (Transform trn in m_Anchors)
        {
            Gizmos.DrawWireSphere(trn.position, 0.05f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + CalcPositionOffset(), 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + CalcPositionOffset());
    }
}
