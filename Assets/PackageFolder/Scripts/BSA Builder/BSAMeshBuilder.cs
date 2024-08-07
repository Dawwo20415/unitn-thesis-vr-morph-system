using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BSAMeshBuilder : MonoBehaviour
{
    public string mesh_name;
    public Mesh mesh;
    public BSACustomMesh bsa_mesh { get { return GenerateBSACustomMesh(); } }
    [HideInInspector]
    public List<Transform> vertices;
    public List<HumanBodyBones> anchors;
    private List<Transform> anchors_trn;
    private List<Vector3> vertices_positions;

    public BSACustomMesh GenerateBSACustomMesh()
    {
        BSACustomMesh tmp = new BSACustomMesh();

        tmp.name = mesh_name;
        tmp.offset = CalcPositionOffset();
        tmp.rot_offset = transform.rotation;

        tmp.vertices = CalculateVertices();
        tmp.triangles = mesh.triangles;
        tmp.anchors = anchors;

        return tmp;
    }

    public void SetAnchors(Animator animator, List<HumanBodyBones> hbb_anchors)
    {
        anchors = new List<HumanBodyBones>(hbb_anchors);
        anchors_trn = new List<Transform>(hbb_anchors.Count);
        foreach (HumanBodyBones hbb in hbb_anchors)
        {
            anchors_trn.Add(animator.GetBoneTransform(hbb));
        }
    }

    private Vector3[] CalculateVertices()
    {
        Vector3[] vert = new Vector3[mesh.vertexCount];

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            vert[i] = vertices[i].position;
        }

        return vert;
    }

    private Vector3 CalcPositionOffset()
    {
        Vector3 midpoint = Vector3.zero;

        foreach (Transform trn in anchors_trn)
        {
            midpoint += trn.position;
        }

        midpoint /= anchors_trn.Count;

        return midpoint - transform.position;
    }

    private void Start()
    {
        mesh.RecalculateNormals();
        vertices_positions = new List<Vector3>(vertices.Count);

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices_positions.Add(Vector3.zero);
        }
    }

    private void Update()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices_positions[i] = vertices[i].position;
        }

        mesh.vertices = vertices_positions.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireMesh(mesh);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.92f, 0.016f, 0.4f);
        Gizmos.DrawMesh(mesh);
        Gizmos.color = Color.red;
        foreach (Transform trn in anchors_trn)
        {
            Gizmos.DrawWireSphere(trn.position, 0.05f);
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + CalcPositionOffset(), 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + CalcPositionOffset());
    }
}
