using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public struct AvatarRetargetingComponents
{
    public Quaternion localA;
    public Quaternion localB;
    public Quaternion fromAtoB;

    public AvatarRetargetingComponents(Quaternion q1, Quaternion q2, Quaternion q3)
    {
        localA = q1;
        localB = q2;
        fromAtoB = q3;
    }

    public AvatarRetargetingComponents(Quaternion q)
    {
        localA = q;
        localB = q;
        fromAtoB = q;
    }

    public static AvatarRetargetingComponents identity { get => new AvatarRetargetingComponents(Quaternion.identity); }
}

public struct AvatarRetargetingPlayable : IAnimationJob
{
    private NativeArray<TransformStreamHandle> m_handles;
    private NativeArray<AvatarRetargetingComponents> m_components;

    public void Setup(Animator source_animator, Animator destination_animator, Transform src_root, Transform dest_root)
    {
        BindSkeleton(destination_animator);
        CalculateTransitions(source_animator, destination_animator, src_root, dest_root);
    }

    private void CalculateTransitions(Animator source_animator, Animator destination_animator, 
                                      Transform src_root,       Transform dest_root)
    {
        m_components = new NativeArray<AvatarRetargetingComponents>((int)HumanBodyBones.LastBone, Allocator.Persistent);

        Dictionary<int, int> source_hbb = MecanimHumanoidExtension.HumanBodyBones2AvatarSkeleton(source_animator);
        Dictionary<int, int> dest_hbb = MecanimHumanoidExtension.HumanBodyBones2AvatarSkeleton(destination_animator);

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            if (source_hbb[i] == -1 || dest_hbb[i] == -1)
            {
                m_components[i] = AvatarRetargetingComponents.identity;
            }
            else
            {
                m_components[i] = FormComponents(source_animator, (HumanBodyBones)i, src_root, 
                                                 destination_animator, (HumanBodyBones)i, dest_root);
            }
        }
    }
    private AvatarRetargetingComponents FormComponents(Animator src_anim, HumanBodyBones src, Transform src_root, Animator dest_anim, HumanBodyBones dest, Transform dest_root)
    {
        Quaternion src_local = GetBoneFromTransform(src_anim.avatar.humanDescription, src_anim.GetBoneTransform(src)).rotation;
        Quaternion dest_local = GetBoneFromTransform(dest_anim.avatar.humanDescription, dest_anim.GetBoneTransform(dest)).rotation;

        Quaternion fromRootToSrc = StackToParentAnimator(src_anim, src, src_root);
        Quaternion fromRootToDest = StackToParentAnimator(dest_anim, dest, dest_root);

        Quaternion fromSrctoDest = QExtension.FromTo(fromRootToSrc, fromRootToDest);

        return new AvatarRetargetingComponents(src_local, dest_local, fromSrctoDest);
    }
    private Quaternion StackToParentAnimator(Animator anim, HumanBodyBones index, Transform root)
    {
        Transform bone = anim.GetBoneTransform(index);
        Quaternion diff = Quaternion.identity;

        do
        {
            Quaternion tpose = GetBoneFromTransform(anim.avatar.humanDescription, bone).rotation;
            diff = tpose * diff;
            bone = bone.parent;
        } while (bone != root);

        return diff;
    }
    private SkeletonBone GetBoneFromTransform(HumanDescription hd, Transform trn)
    {
        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            if (hd.skeleton[i].name == trn.name)
            {
                return hd.skeleton[i];
            }
        }
        Debug.Log("Have not found the bone");
        return new SkeletonBone();
    }

    private void BindSkeleton(Animator animator)
    {
        m_handles = new NativeArray<TransformStreamHandle>((int)HumanBodyBones.LastBone, Allocator.Persistent);

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            m_handles[i] = animator.BindStreamTransform(animator.GetBoneTransform((HumanBodyBones)i));
        }
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Quaternion a = m_handles[i].GetLocalRotation(stream);
            Quaternion b = QExtension.ChangeFrame(Quaternion.Inverse(m_components[i].localA) * a, m_components[i].fromAtoB);

            m_handles[i].SetLocalRotation(stream, m_components[i].localB * b);
        }
    }
}
