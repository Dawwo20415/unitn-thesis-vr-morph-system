using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshShape
{
    public Mesh mesh;
    public Transform transform;

    public MeshShape(GameObject obj)
    {
        transform = obj.transform;
        mesh = obj.GetComponent<MeshFilter>().mesh;
    }
}

public class BodySturfaceApproximation
{
    public int size { get; }
    public int customTrisCount { get => m_trisNumber; }
    public int cylindersCount { get => m_cylinders.Count; }
    public List<MeshShape> custom { get => m_customMeshes; }
    public List<Transform> cylinders { get => m_cylinders; }

    private List<MeshShape> m_customMeshes;
    private int m_trisNumber;
    private List<Transform> m_cylinders;

    public BodySturfaceApproximation(List<GameObject> custom_meshes, List<GameObject> cylinders)
    {
        m_customMeshes = new List<MeshShape>(custom_meshes.Count);
        m_cylinders = new List<Transform>(cylinders.Count);
        size = 0;
        m_trisNumber = 0;

        foreach (GameObject obj in custom_meshes)
        {
            MeshShape mShape = new MeshShape(obj);
            m_customMeshes.Add(mShape);
            size += mShape.mesh.triangles.Length / 3;
            m_trisNumber += mShape.mesh.triangles.Length / 3;
        }

        foreach (GameObject obj in cylinders)
        {
            m_cylinders.Add(obj.transform);
            size++;
        }
    }
}
