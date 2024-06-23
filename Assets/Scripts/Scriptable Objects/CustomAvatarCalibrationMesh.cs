using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCalibrationMesh : ScriptableObject
{
    public List<HumanBodyBones> anchors;

    public Vector3 position_offset;
    public Quaternion rotation_offset;

    public virtual Mesh getMesh()
    {
        return new Mesh();
    }

    public virtual Vector3 getScale()
    {
        return Vector3.one;
    }
}

[System.Serializable]
public struct ExtremitiesPlaneData
{
    public HumanBodyBones bone;
    public Vector3 position_offset;
    public Vector3 scale;
    public Quaternion rotation_offset;
}

[CreateAssetMenu(fileName = "Calibration Mesh", menuName = "ScriptableObjects/Avatar Calibration/Cal Mesh")]
public class CustomAvatarCalibrationMesh : AvatarCalibrationMesh
{
    public List<Vector3> points;
    public List<ExtremitiesPlaneData> planes;
    public CalibrationMeshDescriptor triangles;
    public string mesh_name;

    public override Mesh getMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = mesh_name;
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.getTrisArray();

        return mesh;
    }
}

public class CapsuleAvatarCalibrationMesh : AvatarCalibrationMesh
{
    public Mesh capsule_mesh;

    public float length;
    public float radius;

    public override Mesh getMesh()
    {
        return capsule_mesh;
    }

    public override Vector3 getScale()
    {
        return new Vector3(radius, length, radius);
    }
}
