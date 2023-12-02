using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarOperationOptitrack : AvatarOperation
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public CustomOptitrackStreamingClient StreamingClient;


    private OptitrackSkeletonDefinition m_skeletonDef;

    public override void Setup()
    {
        base.Setup();
    }

    public override void Compute(Dictionary<int, Transform> m_boneObjectMap)
    {
        OptitrackSkeletonState skelState = StreamingClient.GetLatestSkeletonState(m_skeletonDef.Id);
        if (skelState != null)
        {
            // Update the transforms of the bone GameObjects.
            for (int i = 0; i < m_skeletonDef.Bones.Count; ++i)
            {
                Int32 boneId = m_skeletonDef.Bones[i].Id;

                OptitrackPose bonePose;
                Transform boneObject;

                bool foundPose = false;
                if (StreamingClient.SkeletonCoordinates == StreamingCoordinatesValues.Global)
                {
                    // Use global skeleton coordinates
                    foundPose = skelState.LocalBonePoses.TryGetValue(boneId, out bonePose);
                }
                else
                {
                    // Use local skeleton coordinates
                    foundPose = skelState.BonePoses.TryGetValue(boneId, out bonePose);
                }

                bool foundObject = m_boneObjectMap.TryGetValue(boneId, out boneObject);
                if (foundPose && foundObject)
                {
                    boneObject.localPosition = bonePose.Position;
                    boneObject.localRotation = bonePose.Orientation;
                }
            }
        }
    }
}
