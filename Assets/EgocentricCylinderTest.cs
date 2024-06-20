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

    // Start is called before the first frame update
    void Start()
    {
        /*
         * Vector3 a = Vector3.up;
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
         * */
    }

    // Update is called once per frame
    void Update()
    {
        BSACoordinates transfer = ProjectOnCylinder();

        ProjectFromCylinder(transfer);
    }

    private BSACoordinates ProjectOnCylinder()
    {
        BSACoordinates result;

        Vector3 p = transform.position;
        Vector3 a = Vector3.up;
        Vector3 b = Vector3.down;
        Vector3 f = c1.forward;

        a = c1.TransformPoint(a);
        b = c1.TransformPoint(b);
        //f = c1.TransformPoint(f);

        float radius = c1.localScale.x / 2;

        Vector3 AB = b - a;
        Vector3 AP = p - a;

        float ABAPdot = Vector3.Dot(AB.normalized, AP);

        Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

        Vector3 JP = p - projection_on_line;

        //Vector3 inJP = JP.normalized * radius;

        Vector3 inJP = JP.normalized * radius;

        float angle_between = Vector3.SignedAngle(f, inJP, AB);
        f = f * radius;
        //Debug.Log("Angle Between = " + angle_between);
        float distance = JP.magnitude - radius;
        Vector3 displacement = JP.normalized * distance;

        result.surfaceProjection = new Vector2(ABAPdot, angle_between);
        result.displacement = displacement / c1_length;
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
