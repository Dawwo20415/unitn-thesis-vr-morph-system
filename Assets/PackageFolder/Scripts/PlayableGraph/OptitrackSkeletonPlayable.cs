using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Animations;

public struct OptitrackSkeletonPlayable : IAnimationJob
{
    private PlayableOptitrackStreamingClient m_client;
    private OptitrackSkeletonDefinition m_skeletonDefinition;
    private Dictionary<Int32, int> m_id2HumanBodyBones;
    private NativeArray<TransformStreamHandle> m_handles;

    public void Setup(Animator animator, PlayableOptitrackStreamingClient client, OptitrackSkeletonDefinition skeletonDefinition, Dictionary<int,GameObject> guide)
    {
        m_client = client;
        m_skeletonDefinition = skeletonDefinition;
        OptitrackId2HumanBodyBones(guide, animator);
        BindSkeleton(animator);
    }

    private void BindSkeleton(Animator animator)
    {
        m_handles = new NativeArray<TransformStreamHandle>((int)HumanBodyBones.LastBone, Allocator.Persistent);

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            m_handles[i] = animator.BindStreamTransform(animator.GetBoneTransform((HumanBodyBones)i));
        }
    }

    private Dictionary<int, int> OptitrackId2HumanBodyBones(Dictionary<int, GameObject> guide, Animator animator)
    {
        Dictionary<int, int> translation = new Dictionary<int, int>(guide.Count);

        foreach ((int key, GameObject obj) in guide)
        {
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                if (animator.GetBoneTransform((HumanBodyBones)i) == obj.transform)
                {
                    translation[key] = i;
                    break;
                }
            }
        }

        return translation;
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        OptitrackSkeletonState skelState = m_client.GetLatestSkeletonState(m_skeletonDefinition.Id);
        if (skelState != null)
        {
            // Update the transforms of the bone GameObjects.
            for (int i = 0; i < m_skeletonDefinition.Bones.Count; ++i)
            {
                Int32 boneId = m_skeletonDefinition.Bones[i].Id;

                OptitrackPose bonePose;

                bool foundPose = false;
                if (m_client.SkeletonCoordinates == StreamingCoordinatesValues.Global)
                {
                    // Use global skeleton coordinates
                    foundPose = skelState.LocalBonePoses.TryGetValue(boneId, out bonePose);
                }
                else
                {
                    // Use local skeleton coordinates
                    foundPose = skelState.BonePoses.TryGetValue(boneId, out bonePose);
                }

                if (foundPose)
                {
                    int index = m_id2HumanBodyBones[boneId];
                    m_handles[index].SetLocalRotation(stream, bonePose.Orientation);
                    m_handles[index].SetLocalPosition(stream, bonePose.Position);
                }
            }
        }
    }
}
