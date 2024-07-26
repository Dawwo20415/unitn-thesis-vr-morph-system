using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BSAMeshBuilder : MonoBehaviour
{
    public Mesh mesh;
    public List<Transform> vertices;
    private List<Vector3> vertices_positions;

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
}
