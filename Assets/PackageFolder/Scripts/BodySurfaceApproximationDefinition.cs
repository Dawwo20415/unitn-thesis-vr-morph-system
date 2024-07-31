using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BSACylinder
{
    public string name;
    public HumanBodyBones start;
    public HumanBodyBones end;
    [Range(0.01f, 0.5f)]
    public float radius;
}

[System.Serializable]
public struct BSACustomMesh
{
    public string name;
    public Vector3 offset;
    public Quaternion rot_offset;

    public List<Vector3> vertices;
    public List<int> triangles;
}


public class BodySurfaceApproximationDefinition : ScriptableObject
{

    public List<BSACylinder> cylinders;
    public List<BSACustomMesh> meshes;

}
