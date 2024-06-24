using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EgocentricCylinderTest : MonoBehaviour
{
    public Transform c1;
    public float c1_length;
    public Transform c2;
    public float c2_length;

    // Update is called once per frame
    void Update()
    {
        BSACoordinates transfer = ProjectOnCylinder();

        ProjectFromCylinder(transfer);
    }

    private BSACoordinates CylinderIn(Transform cylinder, Vector3 pos, float displacement_weight)
    {
        BSACoordinates result;

        //Define Cylinder Extremities and Forward direction
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;
        Vector3 f = cylinder.forward;

        //Transform from local space to world space
        a = cylinder.TransformPoint(a);
        b = cylinder.TransformPoint(b);

        float radius = cylinder.localScale.x / 2;

        Vector3 AB = b - a;
        Vector3 AP = pos - a;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

        Vector3 JP = pos - projection_on_line;
        Vector3 inJP = JP.normalized * radius;
        float angle_between = Vector3.SignedAngle(f, inJP, AB);

        float distance = JP.magnitude - radius;
        Vector3 displacement = JP.normalized * distance;

        result.surfaceProjection = new Vector2(ABAPdot, angle_between);
        result.displacement = displacement / displacement_weight;
        result.weight = 1 / distance;

        {
            Debug.DrawLine(a, a + AP, Color.blue, Time.deltaTime, false);
            Debug.DrawLine(a, projection_on_line, Color.green, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line, projection_on_line + f, Color.black, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line, projection_on_line + inJP, Color.red, Time.deltaTime, false);
            Debug.DrawLine(projection_on_line + inJP, projection_on_line + inJP + displacement, Color.magenta, Time.deltaTime, false);
        }

        return result;
    }

    private BSACoordinates ProjectOnCylinder()
    {
        return CylinderIn(c1, transform.position, c1_length);
    }

    private void ProjectFromCylinder(BSACoordinates bsa)
    {
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;

        a = c2.TransformPoint(a);
        b = c2.TransformPoint(b);

        Vector3 AB = b - a;

        float radius = c2.localScale.x / 2;

        Vector3 direction = Quaternion.AngleAxis(bsa.surfaceProjection.y, AB) * c2.forward;
        Vector3 toSurface = direction * radius;
        Vector3 displacement = direction * bsa.displacement.magnitude * c2_length;

        Vector3 proj_point = AB.normalized * bsa.surfaceProjection.x;

        Debug.DrawLine(a, a + proj_point, Color.green, Time.deltaTime, false);
        Debug.DrawLine(a + proj_point, a + proj_point + (c2.forward * radius), Color.black, Time.deltaTime, false);
        Debug.DrawLine(a + proj_point, a + proj_point + toSurface, Color.red, Time.deltaTime, false);
        Debug.DrawLine(a + proj_point + toSurface, a + proj_point + toSurface + displacement, Color.magenta, Time.deltaTime, false);
    }
}
