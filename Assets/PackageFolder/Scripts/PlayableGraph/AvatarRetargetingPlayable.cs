using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

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

    public override string ToString()
    {
        return "Local A: " + QExtension.PrintEuler(localA) + " | Local B: " + QExtension.PrintEuler(localB) + " | A to B: " + QExtension.PrintEuler(fromAtoB);
    }

    public string ToStringExtended()
    {
        return "Local A: " + QExtension.Print(localA) + " | Local B: " + QExtension.Print(localB) + " | A to B: " + QExtension.Print(fromAtoB);
    }
}

public struct AvatarRetargetingJob : IAnimationJob
{
    [ReadOnly] private NativeArray<Quaternion> m_sharedRotations;
    [ReadOnly] private NativeArray<Vector3> m_sharedPositions;
    private NativeArray<TransformStreamHandle> m_handles;
    private NativeArray<AvatarRetargetingComponents> m_components;
    private int m_size;

    public void Setup(NativeArray<Quaternion> sharedQ, NativeArray<Vector3> sharedV, List<HumanBodyBones> common_bones, Animator source_animator, Animator destination_animator, Transform src_root, Transform dest_root)
    {
        m_sharedRotations = sharedQ;
        m_sharedPositions = sharedV;
        m_size = common_bones.Count;
        BindSkeleton(ref common_bones, destination_animator);
        CalculateTransitions(ref common_bones, source_animator, destination_animator, src_root, dest_root);
    }

    private void CalculateTransitions(ref List<HumanBodyBones> common_bones, Animator source_animator, Animator destination_animator, 
                                      Transform src_root,       Transform dest_root)
    {
        m_components = new NativeArray<AvatarRetargetingComponents>(common_bones.Count, Allocator.Persistent);

        for (int i = 0; i < common_bones.Count; i++)
        {
            m_components[i] = FormComponents(common_bones[i], source_animator, src_root, 
                                                 destination_animator, dest_root);
        }
    }
    private AvatarRetargetingComponents FormComponents(HumanBodyBones bone, Animator src_anim, Transform src_root, Animator dest_anim, Transform dest_root)
    {
        Quaternion src_local = GetBoneFromTransform(src_anim.avatar.humanDescription, src_anim.GetBoneTransform(bone)).rotation;
        Quaternion dest_local = GetBoneFromTransform(dest_anim.avatar.humanDescription, dest_anim.GetBoneTransform(bone)).rotation;

        Quaternion fromRootToSrc = StackToParentAnimator(src_anim, bone, src_root);
        Quaternion fromRootToDest = StackToParentAnimator(dest_anim, bone, dest_root);

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
    private void BindSkeleton(ref List<HumanBodyBones> common_bones, Animator animator)
    {
        m_handles = new NativeArray<TransformStreamHandle>(common_bones.Count, Allocator.Persistent);

        for (int i = 0; i < common_bones.Count; i++)
        {
            m_handles[i] = animator.BindStreamTransform(animator.GetBoneTransform(common_bones[i]));
        }
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < m_size; i++)
        {
            Quaternion a = m_sharedRotations[i];
            Quaternion b = QExtension.ChangeFrame(Quaternion.Inverse(m_components[i].localA) * a, m_components[i].fromAtoB);
            m_handles[i].SetLocalRotation(stream, m_components[i].localB * b);
            m_handles[i].SetLocalPosition(stream, m_sharedPositions[i]);
        }
    }

    public void Dispose()
    {
        m_components.Dispose();
        m_handles.Dispose();
    }
}
