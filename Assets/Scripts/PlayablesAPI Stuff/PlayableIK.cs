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
        //Vector3 goal = m_target.GetTarget();
        Vector3 goal = m_Targets[0].GetPosition(stream);
        //EE = EndEffector
        Vector3 currentEE = m_Bones[0].GetPosition(stream);
        float distance = (currentEE - goal).magnitude;

        int iterations = 0;
        do
        {
            //Triangular Loop
            for (int i = 1; i < m_Bones.Length; i++)
            {
                for (int j = i; j >= 1; j--)
                {
                    if (j == 2)
                    {
                        RotateAdjustBone(stream, m_Bones[j], m_Bones[j-1], currentEE, m_Targets[j-1].GetPosition(stream), goal);
                    } else
                    {
                        RotateBone(stream, m_Bones[j], currentEE, goal);
                    }
                    currentEE = m_Bones[0].GetPosition(stream);
                    distance = (currentEE - goal).magnitude;

                    if (distance <= m_SqrDistError)
                        return;
                }
            }
            iterations++;
        } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
        
    }

    private void RotateBone(AnimationStream stream, TransformStreamHandle bone, Vector3 effector, Vector3 eeGoal)
    {
        Vector3 bonePosition = bone.GetPosition(stream);
        Quaternion boneRotation = bone.GetRotation(stream);

        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = eeGoal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
        bone.SetRotation(stream, fromToRotation * boneRotation);
    }

    private void RotateAdjustBone(AnimationStream stream, TransformStreamHandle bone, TransformStreamHandle nextBone, Vector3 effector, Vector3 boneGoal, Vector3 eeGoal)
    {
        Vector3 bonePosition = bone.GetPosition(stream);
        Quaternion boneRotation = bone.GetRotation(stream);

        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = eeGoal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);

        //Adjustment
        Vector3 boneToGoal = boneGoal - bonePosition;
        Vector3 boneToNext = nextBone.GetPosition(stream) - bonePosition;
        Vector3 nGoal = Vector3.Cross(boneToGoal, bonePosition);
        Vector3 nCurrent = Vector3.Cross(boneToNext, bonePosition);
        Quaternion boneTargetAdjust = Quaternion.AngleAxis(-Vector3.Angle(boneToNext, boneToGoal), boneToEEGoal);

        bone.SetRotation(stream, fromToRotation * boneTargetAdjust * boneRotation);
    }

    public void Dispose()
    {
        m_Bones.Dispose();
        m_Targets.Dispose();
    }
}
