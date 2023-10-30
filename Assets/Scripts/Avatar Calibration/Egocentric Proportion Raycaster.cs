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
    public bool show_normals;
    public bool show_projections;

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
            
        }
    }

    void Update()
    {
        CastRays();
    }

    bool isInsideTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        float w1u = a.x * (c.y - a.y) + ((p.y - a.y) * (c.x - a.x)) - (p.x * (c.y - a.y));
        float w1d = ((b.y - a.y) * (c.x - a.x)) - ((b.x - a.x) * (c.y - a.y));
        float w1 = w1u / w1d;

        float w2 = ((p.y - a.y - w1) * (b.y - a.y)) / (c.y - a.y);

        if (w1 < 0.0f)
            return false;
        if (w2 < 0.0f)
            return false;
        if ((w1 + w2) > 0.0f)
            return false;
        return true;
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
            foreach (BodyShape shape in custom_meshes)
            {
                Mesh mesh = shape.mesh;
                Vector3 pos = shape.transform.position;
                Quaternion rot = shape.transform.rotation;

                for (int i = 0; i < mesh.triangles.Length / 3; i++)
                {
                    Vector3 p1 = mesh.vertices[mesh.triangles[3 * i]] + pos;
                    Vector3 p2 = mesh.vertices[mesh.triangles[(3 * i) + 1]] + pos;
                    Vector3 p3 = mesh.vertices[mesh.triangles[(3 * i) + 2]] + pos;

                    p1 = rot * p1;
                    p2 = rot * p2;
                    p3 = rot * p3;

                    Debug.DrawLine(p1, p1 + new Vector3(0,1,0));
                    Debug.DrawLine(p2, p2 + new Vector3(0, 1, 0));
                    Debug.DrawLine(p3, p3 + new Vector3(0, 1, 0));

                    Vector3 face_normal = Vector3.Cross(p2-p1, p3-p1).normalized;
                    Vector3 midpoint = ((p1 + p2 + p3) / 3);

                    if (show_normals)
                    {
                        Debug.DrawLine(midpoint, midpoint + face_normal, Color.magenta, Time.deltaTime, true);
                    }

                    Vector3 v = joint.position - midpoint;
                    float n = Vector3.Dot(v, face_normal);
                    Vector3 projection = joint.position - (face_normal * n);

                    if (show_projections)
                    {
                        if (isInsideTriangle(projection, p1, p2, p3))
                            Debug.DrawLine(projection, joint.position, Color.green, Time.deltaTime, true);
                    }

                }
            }

            foreach (BodyShape mesh in capsule_meshes)
            {

            }
        }
             
    }
}
