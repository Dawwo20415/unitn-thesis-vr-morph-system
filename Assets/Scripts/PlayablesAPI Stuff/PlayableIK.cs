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

    public Vector3 GetTarget() { return input.GetTarget() + displacement; }

}

public struct ExtractJoint : IAnimationJob, IKTarget
{
    private Vector3 position;
    private TransformStreamHandle joint;
    private TransformStreamHandle root;

    public void Setup(Animator animator, HumanBodyBones b)
    {
        joint = animator.BindStreamTransform(animator.GetBoneTransform(b));
        root = animator.BindStreamTransform(animator.avatarRoot);
    }

    public Vector3 GetTarget() { return position; }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        position = VExtension.FrameChildToParent(root.GetPosition(stream), root.GetRotation(stream), joint.GetPosition(stream));
    }
}

public struct PlayableIK : IAnimationJob
{
    private NativeArray<TransformStreamHandle> m_Bones;
    private float m_SqrDistError;
    private int m_MaxIterationCount;
    private IKTarget m_target;

    public void setup(Animator animator, IKTarget target)
    {
        m_Bones = new NativeArray<TransformStreamHandle>(4, Allocator.Persistent);

        {
            //TEMPORARY MANUAL DECLARATION
            m_Bones[0] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand));
            m_Bones[1] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
            m_Bones[2] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            m_Bones[3] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftShoulder));
        }

        m_target = target;
        m_SqrDistError = 1.0f;
        m_MaxIterationCount = 10;

    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        
    }

    private Quaternion RotateBone()
    {
        return Quaternion.identity;
    }
}
