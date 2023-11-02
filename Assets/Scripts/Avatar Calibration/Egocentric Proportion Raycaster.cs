using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class EgocentricProportionRaycaster : MonoBehaviour
{

    class BodyShape
    {
        public Mesh mesh;
        public Transform transform;

        public BodyShape (Transform trn, Mesh m)
        {
            transform = trn;
            mesh = m;
        }
    }

    class ReferencePoint
    {
        int face_index;
        Vector3 coordinates;

        ReferencePoint(int a, Vector3 b)
        {
            face_index = a;
            coordinates = b;
        }
    }

    public List<GameObject> custom_meshes_obj;
    private List<BodyShape> custom_meshes;
    public List<GameObject> capsule_meshes_obj;
    private List<BodyShape> capsule_meshes;
    public List<Transform> joints;


    [Header("Debugging")]
    public bool do_customs;
    public bool do_capsules;
    public bool show_normals;
    public bool show_projections;
    public bool show_only_projections_inside_face;

    void Start()
    {
        custom_meshes = new List<BodyShape>();
        capsule_meshes = new List<BodyShape>();

        foreach (GameObject obj in custom_meshes_obj)
        {
            BodyShape shape = new BodyShape(obj.transform, obj.GetComponent<MeshFilter>().mesh);
            custom_meshes.Add(shape);
        }

        foreach (GameObject obj in capsule_meshes_obj)
        {
            BodyShape shape = new BodyShape(obj.transform, obj.GetComponent<MeshFilter>().mesh);
            capsule_meshes.Add(shape);
        }
    }

    void Update()
    {
        CastRays();
    }

    void CastRays()
    {
        // For each limb joint
        // calculate the "relative displacement vectors" of each joint to each mesh face and to the limb capsules
        // Each point of the face origin of the projection is also saved as a "reference point"
        // Each reference point has an importance value "lambda", the value is determined in inverse proportion to the displacement vector magnitude
        // All the importance values for a joint are normalized linearly so that their sum is 1 (for each joint)
        // Reference points are stored as barycentric coordinates for triangles and cylindrical coordinates for capsules
        // Capsules seem to be considered as like 1 triangle

        //Ground foot projection is also kinda considered it's own thing but not really
        // And is stored relative to the root reference point

        // For the limb ectremities orientation
        // Decompose the surface normals of the limb extremities into a weigthed sum of "surface relative angular deviations" 
        // Questo non lo ho capito ttroppo bene

        

        foreach (Transform joint in joints)
        {
            if (!do_customs)
                goto capsules;

            foreach (BodyShape shape in custom_meshes)
            {
                Mesh mesh = shape.mesh;
                Vector3 pos = shape.transform.position;
                Quaternion rot = shape.transform.rotation;

                for (int i = 0; i < mesh.triangles.Length / 3; i++)
                {
                    Vector3 p1 = mesh.vertices[mesh.triangles[3 * i]];
                    Vector3 p2 = mesh.vertices[mesh.triangles[(3 * i) + 1]];
                    Vector3 p3 = mesh.vertices[mesh.triangles[(3 * i) + 2]];

                    p1 = shape.transform.TransformPoint(p1);
                    p2 = shape.transform.TransformPoint(p2);
                    p3 = shape.transform.TransformPoint(p3);

                    Vector3 face_normal = Vector3.Cross(p2-p1, p3-p1).normalized;
                    Vector3 midpoint = ((p1 + p2 + p3) / 3);

                    if (show_normals)
                    {
                        Debug.DrawLine(midpoint, midpoint + face_normal, Color.magenta, Time.deltaTime, true);
                    }

                    Vector3 v = joint.position - midpoint;
                    float n = Vector3.Dot(v, face_normal);
                    Vector3 projection = joint.position - (face_normal * n);

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

                    if (show_projections)
                    {
                        if (!show_only_projections_inside_face || (w1 > 0.0f && w2 > 0.0f && (w1 + w2) < 1.0f))
                        {
                            Debug.DrawLine(projection, joint.position, Color.green, Time.deltaTime, true);
                            Debug.DrawLine(a, a + (v1 * w2), Color.red, Time.deltaTime, true);
                            Debug.DrawLine(a + (v1 * w2), (a + (v1 * w2)) + (v0 * w1), Color.blue, Time.deltaTime, true);
                        }
                    }

                    float lambda = 1 / (projection - joint.position).magnitude;

                }
            }

            capsules:
            if (!do_capsules) {continue;}

            foreach (BodyShape shape in capsule_meshes)
            {
                Vector3 a = Vector3.up;
                Vector3 b = Vector3.down;
                Vector3 p = joint.position;

                a = shape.transform.TransformPoint(a);
                b = shape.transform.TransformPoint(b);

                float radius = shape.transform.localScale.x;

                Vector3 AB = b - a;
                Vector3 AP = p - a;

                float ABAPdot = Vector3.Dot(AB.normalized, AP);

                Vector3 projection_on_line = a + (AB.normalized * ABAPdot);

                Vector3 to_projection = (p - projection_on_line).normalized * radius;

                Debug.DrawLine(a, projection_on_line, Color.green, Time.deltaTime, false);
                Debug.DrawLine(projection_on_line, projection_on_line + to_projection, Color.red, Time.deltaTime, false);
                Debug.DrawLine(p, projection_on_line + to_projection, Color.blue, Time.deltaTime, false);

            }
        }    
    }
}
