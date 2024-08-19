using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections.LowLevel.Unsafe;

public struct OptitrackSkeletonJob : IAnimationJob
{
    private PlayableOptitrackStreamingClient m_client;
    private OptitrackSkeletonDefinition m_skeletonDefinition;
    private Dictionary<Int32, int> m_id2StreamHandle;
    private NativeArray<TransformStreamHandle> m_handles;

    public void Setup(Animator animator, PlayableOptitrackStreamingClient client, OptitrackSkeletonDefinition skeletonDefinition, Dictionary<Int32, int> guide)
    {
        m_client = client;
        m_skeletonDefinition = skeletonDefinition;
        BindSkeleton(animator, guide);
    }

    public void RebindSkeleton(Animator animator, Dictionary<Int32, int> table)
    {
        m_handles.Dispose();
        BindSkeleton(animator, table);
    }

    private void BindSkeleton(Animator animator, Dictionary<Int32, int> table)
    {
        m_handles = new NativeArray<TransformStreamHandle>(table.Count, Allocator.Persistent);
        m_id2StreamHandle = new Dictionary<Int32, int>(table.Count);

        int k = 0;
        foreach ((Int32 id, int hbb) in table)
        {
            Transform trn = animator.GetBoneTransform((HumanBodyBones)hbb);
            m_handles[k] = animator.BindStreamTransform(trn);
            m_id2StreamHandle[id] = k;
            k++;
        }
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
                    int index = -1;
                    m_id2StreamHandle.TryGetValue(boneId, out index);
                    if (index != -1)
                    {
                        m_handles[index].SetLocalRotation(stream, bonePose.Orientation);
                        m_handles[index].SetLocalPosition(stream, bonePose.Position);
                    }  
                }
            }
        }
    }

    public void Dispose()
    {
        m_handles.Dispose();
    }
}
