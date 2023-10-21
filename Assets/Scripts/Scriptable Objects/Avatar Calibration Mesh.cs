using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCalibrationMesh : ScriptableObject
{
    List<string> avatar_reference_points;

    public Vector3 position_offset;
    public Quaternion rotation_offset;

    public virtual Mesh getMesh()
    {
        return new Mesh();
    }
}
