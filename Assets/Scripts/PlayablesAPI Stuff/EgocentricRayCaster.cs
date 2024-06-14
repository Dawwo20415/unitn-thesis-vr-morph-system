using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgocentricRayCaster : MonoBehaviour
{
    [System.Serializable]
    public struct DebugStruct
    {
        public bool drawCylinderRays;
        public bool drawNormals;
        public bool drawMeshProjections;
        public bool drawOnlyInContour;

        public DebugStruct(bool m)
        {
            drawCylinderRays = m;
            drawNormals = m;
            drawMeshProjections = m;
            drawOnlyInContour = m;
        }
    }

    private struct MeshShape
    {
        public Mesh mesh;
        public Transform transform;

        public MeshShape (GameObject obj)
        {
            transform = obj.transform;
            mesh = obj.GetComponent<MeshFilter>().mesh;
        }
    }

    private List<MeshShape> m_customMeshes;
    private List<Transform> m_cylinders;
    private DebugStruct m_debugStruct;

    public string test_string = "Hello";

    public void Setup(List<GameObject> custom_meshes, List<GameObject> cylinders, DebugStruct deb)
    {
        m_customMeshes = new List<MeshShape>(custom_meshes.Count);
        m_cylinders = new List<Transform>(cylinders.Count);
        m_debugStruct = deb;

        foreach(GameObject obj in custom_meshes)
        {
            m_customMeshes.Add(new MeshShape(obj));
        }

        foreach (GameObject obj in cylinders)
        {
            m_cylinders.Add(obj.transform);
        }
    }

    public Vector3 Cast()
    {
        foreach (MeshShape shape in m_customMeshes)
        {
            MeshRaycast(shape);
        }

        foreach (Transform trn in m_cylinders)
        {
            CylinderRaycast(trn);
        }

        return Vector3.one;
    }

    private void MeshRaycast(MeshShape shape)
    {
        Mesh mesh = shape.mesh;
        for (int i = 0; i < mesh.triangles.Length / 3; i++)
        {
            Vector3 p1 = mesh.vertices[mesh.triangles[(3 * i)]];
            Vector3 p2 = mesh.vertices[mesh.triangles[(3 * i) + 1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[(3 * i) + 2]];

            p1 = shape.transform.TransformPoint(p1);
            p2 = shape.transform.TransformPoint(p2);
            p3 = shape.transform.TransformPoint(p3);

            Vector3 face_normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
            Vector3 midpoint = ((p1 + p2 + p3) / 3);

            if (m_debugStruct.drawNormals)
            {
                Debug.DrawLine(midpoint, midpoint + face_normal, Color.magenta, Time.deltaTime, true);
            }

            Vector3 v = transform.position - midpoint;
            float n = Vector3.Dot(v, face_normal);
            Vector3 projection = transform.position - (face_normal * n);

            Vector3 p = projection;
            Vector3 a = p1;
            Vector3 b = p2;
            Vector3 c = p3;

            // Baycentric Coordiante solver from "Christer Ericson's Real-Time Collision Detection"
            Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            
            if (m_debugStruct.drawMeshProjections)
            {

                float denom = d00 * d11 - d01 * d01;
                float w1 = (d11 * d20 - d01 * d21) / denom;
                float w2 = (d00 * d21 - d01 * d20) / denom;
                if (!m_debugStruct.drawOnlyInContour || (w1 > 0.0f && w2 > 0.0f && (w1 + w2) < 1.0f))
                {
                    Debug.DrawLine(projection, transform.position, Color.green, Time.deltaTime, true);
                    Debug.DrawLine(a, a + (v1 * w2), Color.red, Time.deltaTime, true);
                    Debug.DrawLine(a + (v1 * w2), (a + (v1 * w2)) + (v0 * w1), Color.blue, Time.deltaTime, true);
                }
            }
            

            float lambda = 1 / (projection - transform.position).magnitude;
        }
    }

    private void CylinderRaycast(Transform trn)
    {
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;
        Vector3 p = transform.position;

        a = trn.TransformPoint(a);
        b = trn.TransformPoint(b);

        float radius = trn.localScale.x;

        Vector3 AB = b - a;
        Vector3 AP = p - a;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

        Vector3 to_projection = (p - projection_on_line).normalized * radius;

        if (m_debugStruct.drawCylinderRays)
        {
            Debug.DrawLine(a, projection_on_line, Color.green, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line, projection_on_line + to_projection, Color.red, Time.deltaTime, false);
            Debug.DrawLine(p, projection_on_line + to_projection, Color.blue, Time.deltaTime, false);
        }
    }
}
