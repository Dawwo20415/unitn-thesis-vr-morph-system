using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EgocentricCoordinatesTest : MonoBehaviour
{
    [Header("Source Triangle")]
    public Transform p1;
    public Transform p2;
    public Transform p3;
    public float arm_length_s;

    [Space]
    [Header("Destination Triangle")]
    public Transform d1;
    public Transform d2;
    public Transform d3;
    public float arm_length_d;

    [Space]
    [Header("Display")]
    [SerializeField] private float projection_magnitude;
    [SerializeField] private Vector2 barycentric_coordinates;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BSACoordinates transfer;

        Vector3 proj = ProjectJoint() - transform.position;
        projection_magnitude = proj.magnitude;
        transfer.displacement = proj / arm_length_s;
        transfer.weight = 1 / proj.magnitude;
        Debug.DrawLine(transform.position, transform.position + proj, Color.green, Time.deltaTime, false);

        Vector2 bCoor = BarycentricCoordinates(ProjectJoint());
        transfer.surfaceProjection = bCoor;
        barycentric_coordinates = bCoor;
        {
            Vector3 a = p1.position;
            Vector3 b = p2.position;
            Vector3 c = p3.position;

            Vector3 v0 = b - a, v1 = c - a;

            Debug.DrawLine(a, a + (v0 * bCoor.x), Color.red, Time.deltaTime, false);
            Debug.DrawLine(a + (v0 * bCoor.x), (a + (v0 * bCoor.x)) + (v1 * bCoor.y), Color.blue, Time.deltaTime, false);
        }

        Vector3 sp = TransferedSurfaceProjection(transfer);
        Vector3 dest = TransferedDisplacementVector(transfer, sp);
        Debug.DrawLine(sp, dest, Color.green, Time.deltaTime, false);
    }

    private Vector3 ProjectJoint()
    {
        Vector3 face_normal = Vector3.Cross(p2.position - p1.position, p3.position - p1.position).normalized;
        Vector3 midpoint = ((p1.position + p2.position + p3.position) / 3);

        Vector3 v = transform.position - midpoint;
        float n = Vector3.Dot(v, face_normal);
        Vector3 projection = transform.position - (face_normal * n);

        return projection;
    }

    private Vector2 BarycentricCoordinates(Vector3 projection)
    {
        Vector3 p = projection;
        Vector3 a = p1.position;
        Vector3 b = p2.position;
        Vector3 c = p3.position;

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

    private Vector3 TransferedSurfaceProjection(BSACoordinates bsa)
    {
        Vector3 a = d1.position;
        Vector3 b = d2.position;
        Vector3 c = d3.position;

        Vector3 v0 = b - a, v1 = c - a;

        {
            Debug.DrawLine(a, a + (v0 * bsa.surfaceProjection.x), Color.red, Time.deltaTime, false);
            Debug.DrawLine(a + (v0 * bsa.surfaceProjection.x), (a + (v0 * bsa.surfaceProjection.x)) + (v1 * bsa.surfaceProjection.y), Color.blue, Time.deltaTime, false);
        }

        return a + (v0 * bsa.surfaceProjection.x) + (v1 * bsa.surfaceProjection.y);
    }

    private Vector3 TransferedDisplacementVector(BSACoordinates bsa, Vector3 surface_point)
    {
        Vector3 face_normal = Vector3.Cross(d2.position - d1.position, d3.position - d1.position).normalized;

        return surface_point + -(face_normal * bsa.displacement.magnitude * arm_length_d);
    }
}
