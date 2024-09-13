using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static EgocentricRayCasterSource;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public static class BSAProjectionOperators
{

    #region Joint to BSC 

    #region Mesh
    public static Vector2 V3toBarycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
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

    public static Vector3 ProjectPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        Vector3 face_normal = Vector3.Cross(b - a, c - a).normalized;
        Vector3 midpoint = ((a + b + c) / 3);

        Vector3 v = p - midpoint;
        float n = Vector3.Dot(v, face_normal);

        return p - (face_normal * n);
    }

    public static float MeshLengthFactor(Vector3 a, Vector3 b, Vector3 c, Vector3 p, Vector2 barycentricProj, float displacement)
    {
        float dis = 1.0f;
        if (barycentricProj.x < 1.0f && barycentricProj.y < 1.0f)
        {
            if (barycentricProj.x > 0.0f && barycentricProj.y > 0.0f)
                dis = displacement;
            else
                dis = (a - p).magnitude;
        }
        else
        {
            if (barycentricProj.x > 1.0f)
            {
                if (barycentricProj.y > 1.0f)
                    dis = (c - p).magnitude;
                else
                    dis = (b - p).magnitude;
            }
        }

        return dis;
    }

    public static BSACoordinates MeshRaycast(Vector3 a, Vector3 b, Vector3 c, Vector3 p, float displacement_weight)
    {
        BSACoordinates result = new BSACoordinates();

        Vector3 projection_point = ProjectPointOnTriangle(a, b, c, p);
        Vector3 displacement_vector = projection_point - p;
        Vector2 barycentric_projection = V3toBarycentric(a, b, c, projection_point);

        float dis = MeshLengthFactor(a, b, c, p, barycentric_projection, displacement_vector.magnitude);

        result.displacement = displacement_vector / displacement_weight;
        if (dis != 0.0f)
            result.weight = 1 / dis;
        else
            result.weight = 2.0f;
        result.surfaceProjection = barycentric_projection;

        return result;
    }
    #endregion

    #region Cylinder
    public static float CylinderLengthFactor(Vector3 a, Vector3 b, Vector3 p, float distance)
    {
        float length = 1.0f;

        if (distance > 0.0f && distance < 1.0f)
            length = distance;
        else
        {
            if (distance < 0.0f)
                length = (a - p).magnitude;
            else
                length = (b - p).magnitude;
        }

        return length;
    }

    public static BSACoordinates CylinderRaycast(Vector3 a, Vector3 b, float radius, Vector3 p, float displacement_weight, Vector3 anchor)
    {
        BSACoordinates result = new BSACoordinates();

        Vector3 AB = b - a;
        Vector3 AP = p - a;
        Vector3 AH = anchor - a;

        Vector3 reference_direction = Vector3.Cross(AH, AB).normalized;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

        Vector3 JP = p - projection_on_line;
        Vector3 inJP = JP.normalized * radius;
        float angle_between = Vector3.SignedAngle(reference_direction, inJP, AB);

        float distance = JP.magnitude - radius;
        Vector3 displacement = JP.normalized * distance;

        float dist = ABAPdot / AB.magnitude;
        result.surfaceProjection = new Vector2(dist, angle_between);
        result.displacement = displacement / displacement_weight;

        float adjusted = CylinderLengthFactor(a, b, p, dist);

        if (adjusted != 0.0f)
            result.weight = 1 / adjusted;
        else
            result.weight = 2.0f;

        return result;
    }
    #endregion

    #endregion

    #region BSC to Joint

    #region Mesh
    public static Vector3 BarycentricToV3(Vector3 a, Vector3 b, Vector3 c, Vector2 coord)
    {
        Vector3 v0 = b - a, v1 = c - a;
        return a + (v0 * coord.x) + (v1 * coord.y);
    }

    public static Vector3 TransferedDisplacementVector(Vector3 a, Vector3 b, Vector3 c, Vector3 displacement, float weight)
    {
        float direction = 1.0f;
        Vector3 face_normal = Vector3.Cross(b - a, c - a).normalized;

        if (Vector3.Dot(displacement.normalized, face_normal.normalized) < 0)
            direction = -1.0f;

        return -(face_normal * displacement.magnitude * weight) * direction;
    }

    public static (Vector3, float) MeshReversal(Vector3 a, Vector3 b, Vector3 c, BSACoordinates bsa, float bone_weight)
    {
        Vector3 position; float weight;

        //position = ConvertToGlobalSpaceTriangle(a, b, c, coordinate);
        //
        Vector3 onPlane = BarycentricToV3(a, b, c, bsa.surfaceProjection);
        Vector3 projection = TransferedDisplacementVector(a, b, c, bsa.displacement, bone_weight);

        position = onPlane + projection;
        //

        weight = bsa.weight;

        return (position, weight);
    }
    #endregion

    #region Cylinder
    public static (Vector3, float) CylinderReversal(Vector3 a, Vector3 b, float radius, BSACoordinates bsa, float bone_weight, Vector3 anchor)
    {
        Vector3 AB = b - a;
        Vector3 AH = anchor - a;

        Vector3 reference_direction = Vector3.Cross(AH, AB).normalized;

        Vector3 direction = Quaternion.AngleAxis(bsa.surfaceProjection.y, AB) * reference_direction;
        Vector3 toSurface = direction * radius;
        Vector3 displacement = direction * bsa.displacement.magnitude * bone_weight;

        Vector3 proj_point = AB * bsa.surfaceProjection.x;

        return (a + proj_point + toSurface + displacement, bsa.weight);
    }
    #endregion

    #endregion
}
