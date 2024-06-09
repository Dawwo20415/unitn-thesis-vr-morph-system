using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class IKTargetJoin : PlayableBehaviour
{

}
public class IKTargetWrap : PlayableBehaviour
{
    private NativeArray<Vector3> m_TargetReference;
    private int m_Index;
    private IKTarget m_Entry;
    private string m_Name;

    public void setup(IKTarget target, NativeArray<Vector3> arr, int i, string name)
    {
        m_Entry = target;
        m_TargetReference = arr;
        m_Index = i;
        m_Name = name;
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) 
    {
        //Debug.Log("Target Wrap [" + m_Name + "] ProcessingFrame");
        m_TargetReference[m_Index] = m_Entry.GetTarget();
    }
}



public struct IKChainJob : IAnimationJob
{
    private NativeArray<TransformStreamHandle> m_Bones;
    //IKChainJob is not responsible for the deletion of this array, it is shared with the chain
    private NativeArray<Vector3> m_Targets;
    
    private float m_SqrDistError;
    private int m_MaxIterationCount;
    private int m_ChainLength;

    public void setup(Animator animator, List<HumanBodyBones> bones, NativeArray<Vector3> targets)
    {
        if (targets.Length > bones.Count || targets.Length < bones.Count - 1)
            throw new UnityException("Bones and targets arrays are of incompatible length | Bones:" + bones.Count + " Targets:" + targets.Length);

        m_Bones = new NativeArray<TransformStreamHandle>(bones.Count, Allocator.Persistent);
        m_Targets = targets;

        for (int i = 0; i < bones.Count; i++)
        {
            m_Bones[i] = animator.BindStreamTransform(animator.GetBoneTransform(bones[i]));
        }

        m_SqrDistError = 0.01f;
        m_MaxIterationCount = 10;
        m_ChainLength = bones.Count;
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        Vector3 goal = m_Targets[0];

        ROTATE_CROSS(stream, goal);
        PRE_2TARGETS(stream);
        CCD_IK(stream, goal);
    }

    private void ROTATE_CROSS(AnimationStream stream, Vector3 goal)
    {
        Quaternion rot;
        float angle = 0.0f;
        for (int i = 1; i < m_ChainLength - 1; i++)
        {
            angle = BetweenNormals(m_Bones[i].GetPosition(stream), m_Bones[i+1].GetPosition(stream), goal, m_Targets[i]);
            Vector3 boneToNext = (m_Bones[i+1].GetPosition(stream) - m_Bones[i].GetPosition(stream)).normalized;
            rot = Quaternion.AngleAxis(angle, boneToNext) * m_Bones[i].GetRotation(stream);
            m_Bones[i].SetRotation(stream, rot);
        }
    }
    private void PRE_2TARGETS(AnimationStream stream)
    {
        Quaternion rot;
        for (int i = m_ChainLength - 1; i > 0; i--)
        {
            rot = RotateBone(m_Bones[i].GetPosition(stream), m_Bones[i-1].GetPosition(stream), m_Targets[i-1]) * m_Bones[i].GetRotation(stream);
            m_Bones[i].SetRotation(stream, rot);
        }
    }
    private void CCD_IK(AnimationStream stream, Vector3 goal)
    {
        float distance = (GetEffector(stream) - goal).magnitude;
        int iterations = 0;
        do
        {
            //Reverse Triangular Loop [1-21-321-4321-Loop]
            for (int i = 1; i < m_ChainLength; i++)
            {
                for (int j = i; j >= 1; j--)
                {
                    Quaternion rot = RotateBone(m_Bones[j].GetPosition(stream), GetEffector(stream), goal) * m_Bones[j].GetRotation(stream);
                    m_Bones[j].SetRotation(stream, rot);
                    distance = (GetEffector(stream) - goal).magnitude;

                    if (distance <= m_SqrDistError)
                        return;
                }
            }
            iterations++;
        } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
    }

    private Vector3 GetEffector(AnimationStream stream)
    {
        return m_Bones[0].GetPosition(stream);
    }
    private Quaternion RotateBone(Vector3 bonePosition, Vector3 effector, Vector3 goal)
    {
        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = goal - bonePosition;

        return Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
    }
    private float BetweenNormals(Vector3 bone, Vector3 prevBone, Vector3 nextBone, Vector3 goal)
    {
        Vector3 boneToNext = nextBone - bone;
        Vector3 boneToPrev = prevBone - bone;
        Vector3 goalToNext = nextBone - goal;
        Vector3 goalToPrev = prevBone - goal;

        Vector3 n1 = Vector3.Cross(boneToNext, boneToPrev).normalized;
        Vector3 n2 = Vector3.Cross(goalToNext, goalToPrev).normalized;

        Vector3 axis = prevBone - nextBone;

        return Vector3.SignedAngle(n1, n2, axis);
    }
    public void Dispose()
    {
        m_Bones.Dispose();
    }
}

