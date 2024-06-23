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

public static class HumanBodyBonesWeightPath
{
    public static HumanBodyBones GetDestination(HumanBodyBones start)
    {
        //Left Arm
        if (start == HumanBodyBones.LeftHand)     { return HumanBodyBones.LeftShoulder; }
        if (start == HumanBodyBones.LeftLowerArm) { return HumanBodyBones.LeftShoulder; }
        if (start == HumanBodyBones.LeftUpperArm) { return HumanBodyBones.LeftShoulder; }
        if (start == HumanBodyBones.LeftShoulder) { return HumanBodyBones.LeftShoulder; }
        //Right Arm
        if (start == HumanBodyBones.RightHand)     { return HumanBodyBones.RightShoulder; }
        if (start == HumanBodyBones.RightLowerArm) { return HumanBodyBones.RightShoulder; }
        if (start == HumanBodyBones.RightUpperArm) { return HumanBodyBones.RightShoulder; }
        if (start == HumanBodyBones.RightShoulder) { return HumanBodyBones.RightShoulder; }
        //Left Leg
        if (start == HumanBodyBones.LeftFoot)     { return HumanBodyBones.LeftUpperLeg; }
        if (start == HumanBodyBones.LeftLowerLeg) { return HumanBodyBones.LeftUpperLeg; }
        if (start == HumanBodyBones.LeftUpperLeg) { return HumanBodyBones.LeftUpperLeg; }
        if (start == HumanBodyBones.LeftToes)     { return HumanBodyBones.LeftUpperLeg; }
        //Right Leg
        if (start == HumanBodyBones.RightFoot)     { return HumanBodyBones.RightUpperLeg; }
        if (start == HumanBodyBones.RightLowerLeg) { return HumanBodyBones.RightUpperLeg; }
        if (start == HumanBodyBones.RightUpperLeg) { return HumanBodyBones.RightUpperLeg; }
        if (start == HumanBodyBones.RightToes)     { return HumanBodyBones.RightUpperLeg; }

        return HumanBodyBones.Hips;
    }
}

public class BodySturfaceApproximation
{
    public int size { get; }
    public int customTrisCount { get => m_trisNumber; }
    public int customMeshCount { get => m_customMeshes.Count; }
    public int cylindersCount { get => m_cylinders.Count; }
    public float GetBoneWeight(HumanBodyBones hbb) { return m_BonesDisplacementWeight[hbb]; }
    public List<MeshShape> custom { get => m_customMeshes; }
    public List<Transform> cylinders { get => m_cylinders; }
    public List<Transform> planes { get => m_planes; }

    private List<MeshShape> m_customMeshes;
    private int m_trisNumber;
    private List<Transform> m_cylinders;
    private Dictionary<HumanBodyBones, float> m_BonesDisplacementWeight;
    private List<Transform> m_planes;

    public BodySturfaceApproximation(Animator animator, List<GameObject> custom_meshes, List<GameObject> cylinders, List<GameObject> planes)
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

        m_BonesDisplacementWeight = new Dictionary<HumanBodyBones, float>((int)HumanBodyBones.LastBone);
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            m_BonesDisplacementWeight[(HumanBodyBones)i] = CalculatePath(animator, (HumanBodyBones)i);
        }

        m_planes = new List<Transform>(planes.Count);
        foreach (GameObject obj in planes)
        {
            m_planes.Add(obj.transform);
        }
    }

    private float CalculatePath(Animator animator, HumanBodyBones start)
    {
        float length = 0.0f;

        Transform trn = animator.GetBoneTransform(start);
        HumanBodyBones target = HumanBodyBonesWeightPath.GetDestination(start);
        Transform dest = animator.GetBoneTransform(target);

        if (trn == dest || trn == null)
            return 1.0f;

        while (trn != dest)
        {
            if (trn.parent == null)
                throw new UnityException("HumanBodyBones path recursion encountered an object without a parent before reaching destination bone!");

            length += GetDistance(trn, trn.parent);
            trn = trn.parent;
        }

        return length;
    }

    private float GetDistance(Transform a, Transform b)
    {
        return Mathf.Abs((a.position - b.position).magnitude);
    }

}
