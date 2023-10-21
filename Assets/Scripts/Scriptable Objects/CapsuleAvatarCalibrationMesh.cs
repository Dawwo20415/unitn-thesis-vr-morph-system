using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CapsuleAvatarCalibrationMesh : AvatarCalibrationMesh
{
    public Mesh capsule_mesh;

    public float length;
    public float radius;

    public override Mesh getMesh()
    {
        return capsule_mesh;
    }
}
