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
    private Dictionary<int, int> human_body_bones2transforms;

    public void Init(AvatarPoseBehaviour playable, Animator animator)
    {
        BindAvatarTransforms(animator);
        posePlayable = playable;
    }

    private void BindAvatarTransforms(Animator animator)
    {
        HumanDescription hd = animator.avatar.humanDescription;

        bones = new NativeArray<TransformStreamHandle>(hd.human.Length, Allocator.Persistent);
        human_body_bones2transforms = new Dictionary<int, int>(hd.human.Length);

        int local_index = 0;
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Transform target = animator.GetBoneTransform((HumanBodyBones)i);
            if (target)
            {
                bones[local_index] = animator.BindStreamTransform(target);
                human_body_bones2transforms[local_index] = i;
                local_index++;
            }
        }

        /*
        int modifiable_bones_counter = 0;

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            if (animator.GetBoneTransform((HumanBodyBones)i) != null)
            {
                modifiable_bones_counter++;
            }
        }

        bones = new NativeArray<TransformStreamHandle>(modifiable_bones_counter, Allocator.Persistent);
        human_body_bones2transforms = new Dictionary<int, int>(modifiable_bones_counter);

        int transform_index = 0;
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            if (animator.GetBoneTransform((HumanBodyBones)i) != null)
            {
                bones[transform_index] = UnityEngine.Animations.AnimatorJobExtensions.BindStreamTransform(animator, animator.GetBoneTransform((HumanBodyBones)i));
                human_body_bones2transforms[transform_index] = i;

                Debug.Log("For animator [" + animator.name + "] linked local index [" + transform_index + "] to HumanBodyBones [" + i + "]");
                transform_index++;
            }
        }

        Debug.Log("For animator [" + animator.name + "] a total of [" + modifiable_bones_counter + "] bones have been found through HumanBodyBones");
        */
    }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            int index = human_body_bones2transforms[i];
            bones[i].SetLocalRotation(stream, posePlayable.GetRotation(index));
            bones[i].SetLocalPosition(stream, posePlayable.GetPosition(index));
        }
    }

    public void Dispose()
    {
        bones.Dispose();
    }
}
