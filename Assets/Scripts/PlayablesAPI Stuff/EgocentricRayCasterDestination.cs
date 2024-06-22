using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgocentricRayCasterDestination : MonoBehaviour
{
    private BodySturfaceApproximation m_BSA;
    private EgocentricRayCasterSource.DebugStruct m_debugStruct;
    private float m_displacementWeight;

    public float m_dist = 0.0f;
    public float m_2dist = 0.0f;

    public void Setup(BodySturfaceApproximation bsa, EgocentricRayCasterSource.DebugStruct egoDebug)
    {
        m_BSA = bsa;
        m_debugStruct = egoDebug;
        m_displacementWeight = 1.0f;
    }

    public Vector3 Calculate(List<BSACoordinates> coordinates)
    {
        Vector3 weighted_sum = Vector3.zero;

        for (int i = 0; i < m_BSA.customTrisCount; i++)
        {
            //weighted_sum += ConvertToGlobalSpaceTriangle(coordinates[i]);
        }

        for (int i = 0; i < m_BSA.cylindersCount; i++)
        {
            weighted_sum += ConvertToGlobalSpaceCylinder(coordinates[i + m_BSA.customTrisCount], m_BSA.cylinders[i], i);
        }

        m_dist = coordinates[coordinates.Count - 1].surfaceProjection.x;
        return weighted_sum / coordinates.Count;
    }

    private Vector3 ConvertToGlobalSpaceTriangle(BSACoordinates bsa)
    {


        return Vector3.zero;
    }

    private Vector3 ConvertToGlobalSpaceCylinder(BSACoordinates bsa, Transform trn, int i)
    {
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;

        a = trn.TransformPoint(a);
        b = trn.TransformPoint(b);

        Vector3 AB = b - a;

        float radius = trn.localScale.x / 2;

        Vector3 direction = Quaternion.AngleAxis(bsa.surfaceProjection.y, AB) * trn.forward;
        Vector3 toSurface = direction * radius;
        Vector3 displacement = direction * bsa.displacement.magnitude * m_displacementWeight;

        //Vector3 proj_point = AB * bsa.surfaceProjection.x;
        Vector3 proj_point = AB * bsa.surfaceProjection.x;
        if (i == 7)
            m_2dist = bsa.surfaceProjection.x;

        Debug.DrawLine(a, a + proj_point, Color.green, Time.deltaTime, false);
        Debug.DrawLine(a + proj_point, a + proj_point + (trn.forward * radius), Color.black, Time.deltaTime, false);
        Debug.DrawLine(a + proj_point, a + proj_point + toSurface, Color.red, Time.deltaTime, false);
        Debug.DrawLine(a + proj_point + toSurface, a + proj_point + toSurface + displacement, Color.magenta, Time.deltaTime, false);

        return a + proj_point + toSurface + displacement;
    }
}
