using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAvatarCalibrationMesh : AvatarCalibrationMesh
{
    public List<Vector3> points;
    public CalibrationMeshDescriptor triangles;

    public override Mesh getMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.getTrisArray();

        return mesh;
    }
}
