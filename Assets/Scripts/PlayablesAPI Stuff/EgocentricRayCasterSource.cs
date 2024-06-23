using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgocentricRayCasterSource : MonoBehaviour
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

    private BodySturfaceApproximation m_BSA;
    private List<BSACoordinates> m_coordinates;
    private DebugStruct m_debugStruct;
    private float m_displacementWeight;
    private Transform m_Hips;

    public void Setup(HumanBodyBones hbb, Animator animator, BodySturfaceApproximation bsa, DebugStruct deb)
    {
        m_debugStruct = deb;
        m_BSA = bsa;
        m_coordinates = new List<BSACoordinates>(m_BSA.size);
        m_displacementWeight = m_BSA.GetBoneWeight(hbb);
        m_Hips = animator.GetBoneTransform(HumanBodyBones.Hips);
    }

    public List<BSACoordinates> Cast()
    {
        m_coordinates.Clear();
        float total_weight_sum = 0.0f;

        foreach (MeshShape shape in m_BSA.custom)
        {
            //For each triangle in the mesh
            for (int i = 0; i < shape.mesh.triangles.Length / 3; i++)
            {
                BSACoordinates bsa = TriangleRaycast(shape, i, m_displacementWeight);
                m_coordinates.Add(bsa);
                total_weight_sum += bsa.weight;
            }
        }

        foreach (Transform trn in m_BSA.cylinders)
        {
            //NOTE ADD PROPER DISPLACEMENT WEIGHT
            BSACoordinates bsa = CylinderRaycast(trn, transform.position, m_displacementWeight);
            m_coordinates.Add(bsa);
            total_weight_sum += bsa.weight;
        }

        for (int i = 0; i < m_coordinates.Count; i++) 
        {
            BSACoordinates tmp = m_coordinates[i];
            tmp.weight = tmp.weight / total_weight_sum;
            m_coordinates[i] = tmp;
        }

        return m_coordinates;
    }

    private BSACoordinates TriangleRaycast(MeshShape shape, int i, float displacement_weight)
    {
        BSACoordinates result = new BSACoordinates();

        Mesh mesh = shape.mesh;

        Vector3 p1 = mesh.vertices[mesh.triangles[(3 * i)]];
        Vector3 p2 = mesh.vertices[mesh.triangles[(3 * i) + 1]];
        Vector3 p3 = mesh.vertices[mesh.triangles[(3 * i) + 2]];

        p1 = shape.transform.TransformPoint(p1);
        p2 = shape.transform.TransformPoint(p2);
        p3 = shape.transform.TransformPoint(p3);

        Vector3 projection_point = SurfaceProjectionPoint(p1, p2, p3);
        Vector3 displacement_vector = projection_point - transform.position;
        Vector2 barycentric_projection = BarycentricCoordinates(p1, p2, p3, projection_point);

        if (m_debugStruct.drawMeshProjections)
        {
            if (!m_debugStruct.drawOnlyInContour || (barycentric_projection.x > 0.0f && barycentric_projection.y > 0.0f && (barycentric_projection.x + barycentric_projection.y) < 1.0f))
            {
                Debug.DrawLine(transform.position, transform.position + displacement_vector, Color.green, Time.deltaTime, false);
                Vector3 v0 = p2 - p1, v1 = p3 - p1;
                Debug.DrawLine(p1, p1 + (v0 * barycentric_projection.x), Color.red, Time.deltaTime, false);
                Debug.DrawLine(p1 + (v0 * barycentric_projection.x), (p1 + (v0 * barycentric_projection.x)) + (v1 * barycentric_projection.y), Color.blue, Time.deltaTime, false);
            }
        }

        result.displacement = displacement_vector / displacement_weight;
        result.weight = 1 / displacement_vector.magnitude;
        result.surfaceProjection = barycentric_projection;

        return result;
    }

    private Vector3 SurfaceProjectionPoint(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 face_normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        Vector3 midpoint = ((p1 + p2 + p3) / 3);

        Vector3 v = transform.position - midpoint;
        float n = Vector3.Dot(v, face_normal);

        if (m_debugStruct.drawNormals)
        {
            Debug.DrawLine(midpoint, midpoint + (face_normal * 0.1f) , Color.magenta, Time.deltaTime, true);
        }

        return transform.position - (face_normal * n);
    }

    private Vector2 BarycentricCoordinates(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
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

        return new Vector2(w1, w2);
    }

    private BSACoordinates CylinderRaycast(Transform cylinder, Vector3 pos, float displacement_weight)
    {
        BSACoordinates result = new BSACoordinates();

        //Define Cylinder Extremities and Forward direction
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;

        //Transform from local space to world space
        a = cylinder.TransformPoint(a);
        b = cylinder.TransformPoint(b);

        float radius = cylinder.localScale.x / 2;

        Vector3 AB = b - a;
        Vector3 AP = pos - a;
        Vector3 AH = m_Hips.position - a;

        Vector3 reference_direction = Vector3.Cross(AH, AB).normalized;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

        Vector3 JP = pos - projection_on_line;
        Vector3 inJP = JP.normalized * radius;
        float angle_between = Vector3.SignedAngle(reference_direction, inJP, AB);

        float distance = JP.magnitude - radius;
        Vector3 displacement = JP.normalized * distance;

        float dist = ABAPdot / AB.magnitude;
        result.surfaceProjection = new Vector2(dist, angle_between);
        result.displacement = displacement / displacement_weight;
        result.weight = 1 / distance;

        if (m_debugStruct.drawCylinderRays)
        {
            //Debug.DrawLine(a, a + AP, Color.blue, Time.deltaTime, false);
            Debug.DrawLine(a, projection_on_line, Color.green, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line, projection_on_line + (reference_direction * radius), Color.black, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line, projection_on_line + inJP, Color.red, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line + inJP, projection_on_line + inJP + displacement, Color.magenta, Time.deltaTime, false);
        }

        return result;
    }
}
