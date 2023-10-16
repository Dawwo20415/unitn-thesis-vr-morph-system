using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCalibrationMesh : ScriptableObject
{
    public List<Vector3> points;
    public CalibrationMeshDescriptor triangles;
}
