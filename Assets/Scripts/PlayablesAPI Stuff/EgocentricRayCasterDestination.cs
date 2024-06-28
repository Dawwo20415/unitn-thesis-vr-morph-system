  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgocentricRayCasterDestination : MonoBehaviour
{
    //public List<float> public_weights;

    private BodySturfaceApproximation m_BSA;
    private EgocentricRayCasterSource.DebugStruct m_debugStruct;
    private float m_displacementWeight;
    private Transform m_Hips;

    GameObject obj;
    Vector3 shortest_normal = Vector3.one;
    float distance = 100.0f;
    float distance_treshold = 0.001f;

    public void Setup(HumanBodyBones hbb, Animator animator, BodySturfaceApproximation bsa, EgocentricRayCasterSource.DebugStruct egoDebug)
    {
        m_BSA = bsa;
        m_debugStruct = egoDebug;
        m_displacementWeight = m_BSA.GetBoneWeight(hbb);
        m_Hips = animator.GetBoneTransform(HumanBodyBones.Hips);

        obj = new GameObject("Target");
        obj.transform.localScale = Vector3.one * 0.1f;
    }

    public Vector3 Calculate(List<BSACoordinates> coordinates)
    {
        //public_weights = new List<float>(coordinates.Count);
        Vector3 weighted_sum = Vector3.zero;
        float weight = 0.0f;

        int t = 0;
        int counter = 0;
        for (int i = 0; i < m_BSA.customMeshCount; i++)
        {
            Mesh mesh = m_BSA.custom[i].mesh;
            Transform trn = m_BSA.custom[i].transform;

            for (int j = 0; j < mesh.triangles.Length / 3; j++) 
            {
                Vector3 p1 = mesh.vertices[mesh.triangles[(3 * j)]];
                Vector3 p2 = mesh.vertices[mesh.triangles[(3 * j) + 1]];
                Vector3 p3 = mesh.vertices[mesh.triangles[(3 * j) + 2]];

                p1 = trn.TransformPoint(p1);
                p2 = trn.TransformPoint(p2);
                p3 = trn.TransformPoint(p3);

                //THE SOLUTION I USING A DISTANCE THE SUM OF ALL 3 VECOTRS IN PROJECTION
                if (coordinates[t + j].displacement.magnitude * m_displacementWeight < distance)
                {
                    distance = coordinates[t + j].displacement.magnitude * m_displacementWeight;
                    shortest_normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
                }
                
                //weighted_sum += ConvertToGlobalSpaceTriangle(p1, p2, p3, coordinates[t + j]) * coordinates[t + j].weight;
                weighted_sum += ConvertToGlobalSpaceTriangle(p1, p2, p3, coordinates[t + j]);
                counter++;
                weight += coordinates[t + j].weight;
                //public_weights.Add(coordinates[t + j].weight);
            }

            t = t + mesh.triangles.Length / 3;
        }

        for (int i = 0; i < m_BSA.cylindersCount; i++)
        {
            if (i + m_BSA.customTrisCount == 43)
                continue;
            //weighted_sum += ConvertToGlobalSpaceCylinder(coordinates[i + m_BSA.customTrisCount], m_BSA.cylinders[i], i) * coordinates[i + m_BSA.customTrisCount].weight;
            weighted_sum += ConvertToGlobalSpaceCylinder(coordinates[i + m_BSA.customTrisCount], m_BSA.cylinders[i], i);
            weight += coordinates[i + m_BSA.customTrisCount].weight;
            //public_weights.Add(coordinates[i + m_BSA.customTrisCount].weight);
            counter++;
        }

        //obj.transform.position = weighted_sum;
        //return weighted_sum / weight;
        obj.transform.position = weighted_sum;
        return weighted_sum / counter;
    }

    private Vector3 ConvertToGlobalSpaceTriangle(Vector3 p1, Vector3 p2, Vector3 p3, BSACoordinates bsa)
    {
        Vector3 onPlane = ConvertBarycentricToGlobal(p1, p2, p3, bsa.surfaceProjection);
        Vector3 projection = TransferedDisplacementVector(p1, p2, p3, bsa.displacement);

        if (m_debugStruct.drawMeshProjections)
        {
            Debug.DrawLine(onPlane, onPlane + projection, Color.green, Time.deltaTime, false);
        }

        return onPlane + projection;
    }

    private Vector3 ConvertBarycentricToGlobal(Vector3 a, Vector3 b, Vector3 c, Vector2 coord) 
    {
        Vector3 v0 = b - a, v1 = c - a;

        if (m_debugStruct.drawMeshProjections)
        {
            Debug.DrawLine(a, a + (v0 * coord.x), Color.red, Time.deltaTime, false);
            Debug.DrawLine(a + (v0 * coord.x), (a + (v0 * coord.x)) + (v1 * coord.y), Color.blue, Time.deltaTime, false);
        }

        return a + (v0 * coord.x) + (v1 * coord.y);
    }

    private Vector3 TransferedDisplacementVector(Vector3 a, Vector3 b, Vector3 c, Vector3 displacement)
    {
        float direction = 1.0f;
        Vector3 face_normal = Vector3.Cross(b - a, c - a).normalized;

        if (Vector3.Dot(displacement.normalized, face_normal.normalized) < 0)
            direction = -1.0f;

        return -(face_normal * displacement.magnitude * m_displacementWeight) * direction;
    }

    private Vector3 ConvertToGlobalSpaceCylinder(BSACoordinates bsa, Transform trn, int i)
    {
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;

        a = trn.TransformPoint(a);
        b = trn.TransformPoint(b);

        Vector3 AB = b - a;
        Vector3 AH = m_Hips.position - a;

        float radius = trn.localScale.x / 2;
        Vector3 reference_direction = Vector3.Cross(AH, AB).normalized;

        Vector3 direction = Quaternion.AngleAxis(bsa.surfaceProjection.y, AB) * reference_direction;
        Vector3 toSurface = direction * radius;
        Vector3 displacement = direction * bsa.displacement.magnitude * m_displacementWeight;

        if (bsa.displacement.magnitude * m_displacementWeight < distance && bsa.displacement.magnitude * m_displacementWeight > 0.001f)
        {
            distance = bsa.displacement.magnitude * m_displacementWeight;
            shortest_normal = direction.normalized;
        }

        //Vector3 proj_point = AB * bsa.surfaceProjection.x;
        Vector3 proj_point = AB * bsa.surfaceProjection.x;

        if (m_debugStruct.drawCylinderRays)
        {
            Debug.DrawLine(a, a + proj_point, Color.green, Time.deltaTime, false);
            Debug.DrawLine(a + proj_point, a + proj_point + (reference_direction * radius), Color.black, Time.deltaTime, false);
            Debug.DrawLine(a + proj_point, a + proj_point + toSurface, Color.red, Time.deltaTime, false);
            Debug.DrawLine(a + proj_point + toSurface, a + proj_point + toSurface + displacement, Color.magenta, Time.deltaTime, false);
        }

        return a + proj_point + toSurface + displacement;
    }

    public Quaternion CompareNormals(HumanBodyBones hbb)
    {
        Quaternion q = Quaternion.identity;
        if (distance <= distance_treshold)
        {
            Debug.Log("Distance " + distance);
            q = Quaternion.FromToRotation(m_BSA.planes[0].up, -shortest_normal);
            q = Quaternion.Lerp(q, Quaternion.identity, distance / distance_treshold);
        } else
        {
            Debug.Log("Failed if distance " + distance);
        }


        return q;
    }
}
