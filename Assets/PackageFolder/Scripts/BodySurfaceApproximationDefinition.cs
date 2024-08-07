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

    public List<HumanBodyBones> anchors;
    public Vector3[] vertices;
    public int[] triangles;
}

[System.Serializable]
public struct BSANormal
{
    public string name;
    public Vector3 pos_offset;
    //Uses Transform.forward as the actual vector to rotate
    public Quaternion rot_offset;
}

public static class HumanBodyBonesWeightPath
{
    public static HumanBodyBones GetDestination(HumanBodyBones start)
    {
        //Left Arm
        if (start == HumanBodyBones.LeftHand) { return HumanBodyBones.LeftShoulder; }
        if (start == HumanBodyBones.LeftLowerArm) { return HumanBodyBones.LeftShoulder; }
        if (start == HumanBodyBones.LeftUpperArm) { return HumanBodyBones.LeftShoulder; }
        if (start == HumanBodyBones.LeftShoulder) { return HumanBodyBones.LeftShoulder; }
        //Right Arm
        if (start == HumanBodyBones.RightHand) { return HumanBodyBones.RightShoulder; }
        if (start == HumanBodyBones.RightLowerArm) { return HumanBodyBones.RightShoulder; }
        if (start == HumanBodyBones.RightUpperArm) { return HumanBodyBones.RightShoulder; }
        if (start == HumanBodyBones.RightShoulder) { return HumanBodyBones.RightShoulder; }
        //Left Leg
        if (start == HumanBodyBones.LeftFoot) { return HumanBodyBones.LeftUpperLeg; }
        if (start == HumanBodyBones.LeftLowerLeg) { return HumanBodyBones.LeftUpperLeg; }
        if (start == HumanBodyBones.LeftUpperLeg) { return HumanBodyBones.LeftUpperLeg; }
        if (start == HumanBodyBones.LeftToes) { return HumanBodyBones.LeftUpperLeg; }
        //Right Leg
        if (start == HumanBodyBones.RightFoot) { return HumanBodyBones.RightUpperLeg; }
        if (start == HumanBodyBones.RightLowerLeg) { return HumanBodyBones.RightUpperLeg; }
        if (start == HumanBodyBones.RightUpperLeg) { return HumanBodyBones.RightUpperLeg; }
        if (start == HumanBodyBones.RightToes) { return HumanBodyBones.RightUpperLeg; }

        return HumanBodyBones.Hips;
    }
}

public class BodySurfaceApproximationDefinition : ScriptableObject
{

    public List<BSACylinder> cylinders;
    public List<BSACustomMesh> meshes;
    public List<BSANormal> normals;
    public List<float> body_proportion_weights;

}
