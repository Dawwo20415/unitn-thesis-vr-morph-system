using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Collections;
using UnityEditor.PackageManager;

public class AvatarPoseBehaviour : PlayableBehaviour
{
    protected NativeArray<Quaternion> source_avatar_bones;
    protected NativeArray<Vector3> source_avatar_positions;
    protected Dictionary<int, int> HBB2Index;
    protected Dictionary<int, bool> HBB2Available;

    public AvatarPoseBehaviour()
    {
        int size = (int)HumanBodyBones.LastBone;
        source_avatar_bones = new NativeArray<Quaternion>(size, Allocator.Persistent);
        source_avatar_positions = new NativeArray<Vector3>(size, Allocator.Persistent);
        HBB2Index = new Dictionary<int, int>(size);
        HBB2Available = new Dictionary<int, bool>(size);

        for (int i = 0; i < size; i++)
        {
            source_avatar_bones[i] = Quaternion.identity;
            source_avatar_positions[i] = Vector3.zero;
            HBB2Index[i] = i;
            HBB2Available[i] = false;
        }
    }

    public virtual Quaternion GetRotation(int hbb_index)
    {
        //Debug.Log("Sending Rotation for index[" + hbb_index + "]");
        return source_avatar_bones[HBB2Index[hbb_index]];
    }

    public virtual Vector3 GetPosition(int hbb_index)
    {
        return source_avatar_positions[HBB2Index[hbb_index]];
    }

    public bool GetBoneStatus(int hbb_index)
    {
        return HBB2Available[hbb_index];
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) { }

    public void Dispose()
    {
        source_avatar_bones.Dispose();
        source_avatar_positions.Dispose();
    }
}

public class AvatarRetargetingBehaviour : AvatarPoseBehaviour
{
    private NativeArray<Quaternion> rotation_offsets;
    private NativeArray<Vector3> position_offsets;
    //Input
    private AvatarPoseBehaviour behaviour;

    public void RetargetingSetup(Avatar source_avatar, Avatar destination_avatar)
    {
        rotation_offsets = new NativeArray<Quaternion>(1, Allocator.Persistent);
        position_offsets = new NativeArray<Vector3>(1, Allocator.Persistent);

        //Initialize offsets & input playableBehaviour
        //Transform this thing in an interface IHumanBodyBonesPose? IHumanPose?
    }

    public override Vector3 GetPosition(int hbb_index)
    {
        return behaviour.GetPosition(hbb_index) + position_offsets[hbb_index];
    }

    public override Quaternion GetRotation(int hbb_index)
    {
        return behaviour.GetRotation(hbb_index) * rotation_offsets[hbb_index];
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) { }
}

public class AvatarTPoseBehaviour : AvatarPoseBehaviour
{
    public void TPoseSetup(Animator animator)
    {
        Dictionary<int, int> tmp = MecanimHumanoidExtension.AvatarSkeleton2HumanBodyBones(animator.avatar.humanDescription, animator);
    
        foreach((int skeleton_index, int HBB_index) in tmp)
        {
            if (HBB_index == -1) { continue; }
            source_avatar_bones[HBB_index] = animator.avatar.humanDescription.skeleton[skeleton_index].rotation;
            source_avatar_positions[HBB_index] = animator.avatar.humanDescription.skeleton[skeleton_index].position;
        }
    }
    
    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) { }

}

public class OptitrackPoseBehaviour : AvatarPoseBehaviour
{
    private PlayableOptitrackStreamingClient client;
    private OptitrackSkeletonDefinition skeleton_definition;
    private Dictionary<Int32, int> id2HumanBodyBones;

    public void OptitrackSetup(PlayableOptitrackStreamingClient streamingClient, OptitrackSkeletonDefinition skeletonDefinition, Dictionary<Int32, int> correspondence)
    {
        client = streamingClient;
        skeleton_definition = skeletonDefinition; 
        id2HumanBodyBones = correspondence;

        foreach ((int key, int value) in id2HumanBodyBones)
        {
            HBB2Available[value] = true;
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        //string to_print = "PrepareFrame" + " | StreamingClient_name:[" + client.name + "] StreamingClient_address[" + client.LocalAddress + "]";
        //Debug.Log(to_print);

        OptitrackSkeletonState skelState = client.GetLatestSkeletonState(skeleton_definition.Id);
        if (skelState != null)
        {
            // Update the transforms of the bone GameObjects.
            for (int i = 0; i < skeleton_definition.Bones.Count; ++i)
            {
                Int32 boneId = skeleton_definition.Bones[i].Id;

                OptitrackPose bonePose;

                bool foundPose = false;
                if (client.SkeletonCoordinates == StreamingCoordinatesValues.Global)
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
                    int index = id2HumanBodyBones[boneId];
                    source_avatar_bones[index] = bonePose.Orientation;
                    source_avatar_positions[index] = bonePose.Position;
                }
            }
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) { }
}
