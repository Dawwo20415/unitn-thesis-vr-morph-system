using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public struct PoseApplyJob : IAnimationJob
{
    private AvatarPoseBehaviour posePlayable;
    private NativeArray<TransformStreamHandle> bones;
    private Dictionary<int, int> transforms2HBB;
    private bool applyPosition;

    public void Init(AvatarPoseBehaviour playable, Animator animator, bool apply_position)
    {
        applyPosition = apply_position;
        BindAvatarTransforms(animator);
        posePlayable = playable;
    }

    private void BindAvatarTransforms(Animator animator)
    {
        HumanDescription hd = animator.avatar.humanDescription;

        Dictionary<int, int> tmp = MecanimHumanoidExtension.AvatarSkeleton2HumanBodyBones(hd, animator);
        int size = BoneSize(tmp);
        bones = new NativeArray<TransformStreamHandle>(size, Allocator.Persistent);
        transforms2HBB = new Dictionary<int, int>(size);

        int local_index = 0;
        foreach ((int skeleton_index, int HBB_index) in tmp)
        {
            if (HBB_index == -1) { continue; } 
            Transform target = animator.GetBoneTransform((HumanBodyBones)HBB_index);
            if (target)
            {
                transforms2HBB[local_index] = HBB_index;
                bones[local_index] = animator.BindStreamTransform(target);
                local_index++;
            }
        }
    }

    private int BoneSize(Dictionary<int,int> dic)
    {
        int size = 0;

        foreach ((int skeleton_index, int HBB_index) in dic)
        {
            if (HBB_index == -1) { continue; }
            size++;
        }

        return size;
    }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            int index = transforms2HBB[i];
            bones[i].SetLocalRotation(stream, posePlayable.GetRotation(index));
            if (applyPosition)
            {
                bones[i].SetLocalPosition(stream, posePlayable.GetPosition(index));
            }           
        }
    }

    public void Dispose()
    {
        bones.Dispose();
    }
}

public struct PoseApplyJobDebug : IAnimationJob
{
    private AvatarPoseBehaviour posePlayable;
    private NativeArray<TransformStreamHandle> bones;
    private Dictionary<int, int> transforms2HBB;
    private Dictionary<int, int> transforms2HDSkeleton;
    private bool applyPosition;
    private Avatar avatar;
    private HumanDescription hd;

    public void Init(AvatarPoseBehaviour playable, Animator animator, bool apply_position)
    {
        avatar = animator.avatar;
        hd = animator.avatar.humanDescription;
        applyPosition = apply_position;
        BindAvatarTransforms(animator);
        posePlayable = playable;
    }

    private void BindAvatarTransforms(Animator animator)
    {
        HumanDescription hd = animator.avatar.humanDescription;

        Dictionary<int, int> tmp = MecanimHumanoidExtension.AvatarSkeleton2HumanBodyBones(hd, animator);
        int size = BoneSize(tmp);
        bones = new NativeArray<TransformStreamHandle>(size, Allocator.Persistent);
        transforms2HBB = new Dictionary<int, int>(size);
        transforms2HDSkeleton = new Dictionary<int, int>(size);

        int local_index = 0;
        foreach ((int skeleton_index, int HBB_index) in tmp)
        {
            if (HBB_index == -1) { continue; }
            Transform target = animator.GetBoneTransform((HumanBodyBones)HBB_index);
            if (target)
            {
                transforms2HBB[local_index] = HBB_index;
                transforms2HDSkeleton[local_index] = skeleton_index;
                bones[local_index] = animator.BindStreamTransform(target);
                local_index++;
            }
        }
    }

    private int BoneSize(Dictionary<int, int> dic)
    {
        int size = 0;

        foreach ((int skeleton_index, int HBB_index) in dic)
        {
            if (HBB_index == -1) { continue; }
            size++;
        }

        return size;
    }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            int index = transforms2HBB[i];
            Debug.Log("Setting internal index [" + i + "," + bones[i].ToString() +"] and requesting HBB [" + index + "," + System.Enum.GetName(typeof(HumanBodyBones),index) + "]");
            if (posePlayable.GetBoneStatus(index) == false)
            {
                int skeleton_index = transforms2HDSkeleton[i];
                Debug.Log("This Bone[" + index + "] is not being updated by optitrack");
                bones[i].SetLocalRotation(stream, hd.skeleton[skeleton_index].rotation);
                bones[i].SetLocalPosition(stream, hd.skeleton[skeleton_index].position);
            } else
            {
                bones[i].SetLocalRotation(stream, posePlayable.GetRotation(index));
                if (applyPosition)
                {
                    bones[i].SetLocalPosition(stream, posePlayable.GetPosition(index));

                }
            }
        }
    }

    public void Dispose()
    {
        bones.Dispose();
    }
}
