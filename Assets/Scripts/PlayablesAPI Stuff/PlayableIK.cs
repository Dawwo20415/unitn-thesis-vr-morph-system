using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Unity.Collections;

public interface IKTarget
{
    public Vector3 GetTarget();
}

public class StaticDisplacement : PlayableBehaviour, IKTarget
{
    IKTarget input;
    Vector3 displacement;

    public void Setup(IKTarget target, Vector3 dis)
    {
        input = target;
        displacement = dis;
    }

    public Vector3 GetTarget() 
    {
        return input.GetTarget() + displacement; 
    }

}

public struct ExtractJoint : IAnimationJob, IKTarget
{
    //Abosilutely hate having to allocate an array for just 1 thing
    private NativeArray<Vector3> position;
    private TransformStreamHandle joint;
    private TransformStreamHandle root;
    private HumanBodyBones bone;

    public void Setup(Animator animator, HumanBodyBones b)
    {
        bone = b;
        joint = animator.BindStreamTransform(animator.GetBoneTransform(b));
        root = animator.BindStreamTransform(animator.avatarRoot);
        position = new NativeArray<Vector3>(1, Allocator.Persistent);
    }

    public Vector3 GetTarget() 
    {
        Debug.Log("Extracted " + VExtension.Print(position[0]));
        return position[0]; 
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        position[0] = joint.GetPosition(stream);
    }

    public void Dispose()
    {
        position.Dispose();
    }
}

public struct PlayableIK : IAnimationJob
{
    private NativeArray<TransformStreamHandle> m_Bones;
    private float m_SqrDistError;
    private int m_MaxIterationCount;
    //private IKTarget m_target;

    private NativeArray<TransformSceneHandle> m_Targets;

    public void setup(Animator animator/*, IKTarget target*/, List<Transform> targets)
    {
        m_Bones = new NativeArray<TransformStreamHandle>(4, Allocator.Persistent);

        {
            //TEMPORARY MANUAL DECLARATION
            m_Bones[0] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand));
            m_Bones[1] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
            m_Bones[2] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            m_Bones[3] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftShoulder));
        }

        m_Targets = new NativeArray<TransformSceneHandle>(4, Allocator.Persistent);

        for (int i = 0; i < 4; i++)
        {
            m_Targets[i] = animator.BindSceneTransform(targets[i]);
        }

        //m_target = target;
        m_SqrDistError = 0.01f;
        m_MaxIterationCount = 10;

    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        Vector3 goal = m_Targets[0].GetPosition(stream);

        ROTATE_CROSS(stream, goal);
        PRE_2TARGETS(stream);
        CCD_IK(stream, goal); 
    }

    private void ROTATE_CROSS(AnimationStream stream, Vector3 goal)
    {
        Quaternion rot;
        float angle = 0.0f;
        for (int i = 1; i < m_Bones.Length - 1; i++)
        {
            angle = BetweenNormals(m_Bones[i].GetPosition(stream), m_Bones[i + 1].GetPosition(stream), goal, m_Targets[i].GetPosition(stream));
            Vector3 boneToNext = (m_Bones[i + 1].GetPosition(stream) - m_Bones[i].GetPosition(stream)).normalized;
            rot = Quaternion.AngleAxis(angle, boneToNext) * m_Bones[i].GetRotation(stream);
            m_Bones[i].SetRotation(stream, rot); 
        }
    }

    private void PRE_2TARGETS(AnimationStream stream)
    {
        Quaternion rot;
        for (int i = m_Bones.Length - 1; i > 0; i--)
        {
            rot = RotateBone(m_Bones[i].GetPosition(stream), m_Bones[i - 1].GetPosition(stream), m_Targets[i - 1].GetPosition(stream)) * m_Bones[i].GetRotation(stream);
            m_Bones[i].SetRotation(stream, rot);
        }
    }

    private void CCD_IK(AnimationStream stream, Vector3 goal)
    {
        float distance = (m_Bones[0].GetPosition(stream) - goal).magnitude;
        int iterations = 0;
        do
        {
            //Reverse Triangular Loop [1-21-321-4321-Loop]
            for (int i = 1; i < m_Bones.Length; i++)
            {
                for (int j = i; j >= 1; j--)
                {
                    Quaternion rot = RotateBone(m_Bones[j].GetPosition(stream), m_Bones[0].GetPosition(stream), goal) * m_Bones[j].GetRotation(stream);
                    m_Bones[j].SetRotation(stream, rot);
                    distance = (m_Bones[0].GetPosition(stream) - goal).magnitude;

                    if (distance <= m_SqrDistError)
                        return;
                }
            }
            iterations++;
        } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
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
        m_Targets.Dispose();
    }
}
