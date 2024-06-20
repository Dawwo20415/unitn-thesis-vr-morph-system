using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BSACoordinates
{
    //Normalized weight of the displacement vector
    public float weight;
    //Normalized vector from Surface Projection to Joint
    public Vector3 displacement;
    //Barycentric/Cylindrical coordinates of the joint projected on mesh
    public Vector2 surfaceProjection;

    public BSACoordinates (float i)
    {
        weight = i;
        displacement = new Vector3(i,i,i);
        surfaceProjection = new Vector2(i, i);
    }
}

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

    public List<BSACoordinates> coord;
    private BodySturfaceApproximation m_BSA;
    private DebugStruct m_debugStruct;

    public string test_string = "Hello";

    public void Setup(List<GameObject> custom_meshes, List<GameObject> cylinders, DebugStruct deb)
    {
        m_debugStruct = deb;
        m_BSA = new BodySturfaceApproximation(custom_meshes, cylinders);
    }

    public List<BSACoordinates> Cast()
    {
        List<BSACoordinates> coordinates = new List<BSACoordinates>(m_BSA.size);

        foreach (MeshShape shape in m_BSA.custom)
        {
            MeshRaycast(shape, ref coordinates);
        }

        foreach (Transform trn in m_BSA.cylinders)
        {
            coordinates.Add(CylinderRaycast(trn));
        }

        coord = coordinates;
        return coordinates;
    }

    private void MeshRaycast(MeshShape shape, ref List<BSACoordinates> coordinates)
    {
        //For each triangle
        for (int i = 0; i < shape.mesh.triangles.Length / 3; i++)
        {
            coordinates.Add(TriangleRaycast(shape, i));
        }
    }

    private BSACoordinates TriangleRaycast(MeshShape shape, int i)
    {
        BSACoordinates result = new BSACoordinates(1.0f);

        Mesh mesh = shape.mesh;

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

        float denom = d00 * d11 - d01 * d01;
        float w1 = (d11 * d20 - d01 * d21) / denom;
        float w2 = (d00 * d21 - d01 * d20) / denom;

        result.displacement = projection - transform.position;
        result.surfaceProjection = new Vector2(w1, w2);

        if (m_debugStruct.drawMeshProjections)
        {
            if (!m_debugStruct.drawOnlyInContour || (w1 > 0.0f && w2 > 0.0f && (w1 + w2) < 1.0f))
            {
                Debug.DrawLine(projection, transform.position, Color.green, Time.deltaTime, true);
                Debug.DrawLine(a, a + (v1 * w2), Color.red, Time.deltaTime, true);
                Debug.DrawLine(a + (v1 * w2), (a + (v1 * w2)) + (v0 * w1), Color.blue, Time.deltaTime, true);
            }
        }
            

        result.weight = 1 / (projection - transform.position).magnitude;

        return result;
    }

    private BSACoordinates CylinderRaycast(Transform trn)
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

        BSACoordinates result = new BSACoordinates(1.0f);

        return result;
    }
}
