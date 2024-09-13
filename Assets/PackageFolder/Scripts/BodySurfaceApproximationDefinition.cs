using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Triangle
{
    public int id;
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
}

public struct Cylinder
{
    public HumanBodyBones a;
    public HumanBodyBones b;
}

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

[System.Serializable]
public struct BSACoordinates
{
    //Normalized weight of the displacement vector
    public float weight;
    //Normalized vector from Surface Projection to Joint
    public Vector3 displacement;
    //Barycentric/Cylindrical coordinates of the joint projected on mesh
    public Vector2 surfaceProjection;

    public BSACoordinates(float i)
    {
        weight = i;
        displacement = new Vector3(i, i, i);
        surfaceProjection = new Vector2(i, i);
    }

    public BSACoordinates(Vector2 sp, Vector3 d, float w)
    {
        weight = w;
        displacement = d;
        surfaceProjection = sp;
    }
}

public class BodySurfaceApproximationDefinition : ScriptableObject
{
    public int coordinateSpan { get => CalculateSpan(); }

    public List<BSACylinder> cylinders;
    public List<BSACustomMesh> meshes;
    public List<BSANormal> normals;
    public List<float> body_proportion_weights;

    public IEnumerable<Triangle> meshTris()
    {
        Triangle result; int k = 0;
        foreach (BSACustomMesh mesh in meshes)
        {
            for (int i = 0; i < mesh.triangles.Length / 3; i++)
            {
                result.a = mesh.vertices[mesh.triangles[(3 * i)]];
                result.b = mesh.vertices[mesh.triangles[(3 * i) + 1]];
                result.c = mesh.vertices[mesh.triangles[(3 * i) + 2]];
                result.id = k;

                yield return result;
            }
            k++;
        }
    }

    public IEnumerable<BSACylinder> cylindersDef()
    {
        foreach (BSACylinder cyl in cylinders)
        {
            yield return cyl;
        }
    }

    private int CalculateSpan()
    {
        int size = 0;

        foreach (BSACustomMesh mesh in meshes)
        {
            size += (mesh.triangles.Length / 3);
        }

        size += cylinders.Count;

        return size;
    }
}

public class BodySurfaceApproximationRuntime
{
    public List<Transform> meshes;

    public BodySurfaceApproximationRuntime(BodySurfaceApproximationDefinition BSAD)
    {
        meshes = new List<Transform>(BSAD.meshes.Count);
    }
}
