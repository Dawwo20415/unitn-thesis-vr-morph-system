using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarOperationOptitrack : AvatarOperation
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public CustomOptitrackStreamingClient StreamingClient;
    [Tooltip("The name of skeleton asset in Motive.")]
    public string SkeletonAssetName = "Skeleton1";

    private OptitrackSkeletonDefinition m_skeletonDef;
    private Dictionary<string, string> m_cachedMecanimBoneNameMap = new Dictionary<string, string>();
    private Dictionary<string, int> m_cachedIndex2Names = new Dictionary<string, int>();

    //This is the same initialization process of the standard optitrack skeleton script
    private void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if (this.StreamingClient == null)
        {
            this.StreamingClient = CustomOptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if (this.StreamingClient == null)
            {
                Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(CustomOptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
                this.enabled = false;
                return;
            }
        }

        this.StreamingClient.RegisterSkeleton(this, this.SkeletonAssetName);

        // Create a lookup from Mecanim anatomy bone names to OptiTrack streaming bone names.
        CacheBoneNameMap(this.StreamingClient.BoneNamingConvention, this.SkeletonAssetName);
        LoadIndex2Names();

        // Retrieve the OptiTrack skeleton definition.
        m_skeletonDef = this.StreamingClient.GetSkeletonDefinitionByName(this.SkeletonAssetName);
        if (m_skeletonDef == null)
        {
            Debug.LogError(GetType().FullName + ": Could not find skeleton definition with the name \"" + this.SkeletonAssetName + "\"", this);
            this.enabled = false;
            return;
        }
    }

    public override void Compute(Dictionary<int, GameObject> m_boneObjectMap)
    {
        OptitrackSkeletonState skelState = StreamingClient.GetLatestSkeletonState(m_skeletonDef.Id);
        if (skelState != null)
        {
            // Update the transforms of the bone GameObjects.
            for (int i = 0; i < m_skeletonDef.Bones.Count; ++i)
            {
                Int32 boneId = m_skeletonDef.Bones[i].Id;
                int humanTraitId = SkeletonDef2HumanTrait(m_skeletonDef.Bones[i].Name);

                OptitrackPose bonePose;
                GameObject boneObject;

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

                bool foundObject = m_boneObjectMap.TryGetValue(humanTraitId, out boneObject);
                if (foundPose && foundObject)
                {
                    boneObject.transform.localPosition = bonePose.Position;
                    boneObject.transform.localRotation = bonePose.Orientation;
                }
            }
        }
    }

    private int SkeletonDef2HumanTrait(string optitrack_name)
    {
        if (m_cachedIndex2Names.ContainsKey(optitrack_name))
        {
            int id;
            m_cachedIndex2Names.TryGetValue(optitrack_name, out id);
            return id;
        }
        return -1;
    }

    private void LoadIndex2Names()
    {
        m_cachedIndex2Names.Clear();

        foreach(KeyValuePair<string,string> opt_pair in m_cachedMecanimBoneNameMap)
        {
            int id = 0;

            for (int i = 0; i < HumanTrait.BoneCount; i++)
            {
                if (HumanTrait.BoneName[i] == opt_pair.Key)
                {
                    id = i;
                    break;
                }
            }

            m_cachedIndex2Names.Add(opt_pair.Value, id);
        }
    }

    private void CacheBoneNameMap(OptitrackBoneNameConvention convention, string assetName)
    {
        m_cachedMecanimBoneNameMap.Clear();

        switch (convention)
        {
            case OptitrackBoneNameConvention.Motive:
                m_cachedMecanimBoneNameMap.Add("Hips", assetName + "_Hip");
                m_cachedMecanimBoneNameMap.Add("Spine", assetName + "_Ab");
                m_cachedMecanimBoneNameMap.Add("Chest", assetName + "_Chest");
                m_cachedMecanimBoneNameMap.Add("Neck", assetName + "_Neck");
                m_cachedMecanimBoneNameMap.Add("Head", assetName + "_Head");

                m_cachedMecanimBoneNameMap.Add("LeftShoulder", assetName + "_LShoulder");
                m_cachedMecanimBoneNameMap.Add("LeftUpperArm", assetName + "_LUArm");
                m_cachedMecanimBoneNameMap.Add("LeftLowerArm", assetName + "_LFArm");
                m_cachedMecanimBoneNameMap.Add("LeftHand", assetName + "_LHand");

                m_cachedMecanimBoneNameMap.Add("RightShoulder", assetName + "_RShoulder");
                m_cachedMecanimBoneNameMap.Add("RightUpperArm", assetName + "_RUArm");
                m_cachedMecanimBoneNameMap.Add("RightLowerArm", assetName + "_RFArm");
                m_cachedMecanimBoneNameMap.Add("RightHand", assetName + "_RHand");

                m_cachedMecanimBoneNameMap.Add("LeftUpperLeg", assetName + "_LThigh");
                m_cachedMecanimBoneNameMap.Add("LeftLowerLeg", assetName + "_LShin");
                m_cachedMecanimBoneNameMap.Add("LeftFoot", assetName + "_LFoot");
                m_cachedMecanimBoneNameMap.Add("LeftToeBase", assetName + "_LToe");

                m_cachedMecanimBoneNameMap.Add("RightUpperLeg", assetName + "_RThigh");
                m_cachedMecanimBoneNameMap.Add("RightLowerLeg", assetName + "_RShin");
                m_cachedMecanimBoneNameMap.Add("RightFoot", assetName + "_RFoot");
                m_cachedMecanimBoneNameMap.Add("RightToeBase", assetName + "_RToe");


                m_cachedMecanimBoneNameMap.Add("Left Thumb Proximal", assetName + "_LThumb1");
                m_cachedMecanimBoneNameMap.Add("Left Thumb Intermediate", assetName + "_LThumb2");
                m_cachedMecanimBoneNameMap.Add("Left Thumb Distal", assetName + "_LThumb3");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Proximal", assetName + "_RThumb1");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Intermediate", assetName + "_RThumb2");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Distal", assetName + "_RThumb3");

                m_cachedMecanimBoneNameMap.Add("Left Index Proximal", assetName + "_LIndex1");
                m_cachedMecanimBoneNameMap.Add("Left Index Intermediate", assetName + "_LIndex2");
                m_cachedMecanimBoneNameMap.Add("Left Index Distal", assetName + "_LIndex3");
                m_cachedMecanimBoneNameMap.Add("Right Index Proximal", assetName + "_RIndex1");
                m_cachedMecanimBoneNameMap.Add("Right Index Intermediate", assetName + "_RIndex2");
                m_cachedMecanimBoneNameMap.Add("Right Index Distal", assetName + "_RIndex3");

                m_cachedMecanimBoneNameMap.Add("Left Middle Proximal", assetName + "_LMiddle1");
                m_cachedMecanimBoneNameMap.Add("Left Middle Intermediate", assetName + "_LMiddle2");
                m_cachedMecanimBoneNameMap.Add("Left Middle Distal", assetName + "_LMiddle3");
                m_cachedMecanimBoneNameMap.Add("Right Middle Proximal", assetName + "_RMiddle1");
                m_cachedMecanimBoneNameMap.Add("Right Middle Intermediate", assetName + "_RMiddle2");
                m_cachedMecanimBoneNameMap.Add("Right Middle Distal", assetName + "_RMiddle3");

                m_cachedMecanimBoneNameMap.Add("Left Ring Proximal", assetName + "_LRing1");
                m_cachedMecanimBoneNameMap.Add("Left Ring Intermediate", assetName + "_LRing2");
                m_cachedMecanimBoneNameMap.Add("Left Ring Distal", assetName + "_LRing3");
                m_cachedMecanimBoneNameMap.Add("Right Ring Proximal", assetName + "_RRing1");
                m_cachedMecanimBoneNameMap.Add("Right Ring Intermediate", assetName + "_RRing2");
                m_cachedMecanimBoneNameMap.Add("Right Ring Distal", assetName + "_RRing3");

                m_cachedMecanimBoneNameMap.Add("Left Little Proximal", assetName + "_LPinky1");
                m_cachedMecanimBoneNameMap.Add("Left Little Intermediate", assetName + "_LPinky2");
                m_cachedMecanimBoneNameMap.Add("Left Little Distal", assetName + "_LPinky3");
                m_cachedMecanimBoneNameMap.Add("Right Little Proximal", assetName + "_RPinky1");
                m_cachedMecanimBoneNameMap.Add("Right Little Intermediate", assetName + "_RPinky2");
                m_cachedMecanimBoneNameMap.Add("Right Little Distal", assetName + "_RPinky3");
                break;
            case OptitrackBoneNameConvention.FBX:
                m_cachedMecanimBoneNameMap.Add("Hips", assetName + "_Hips");
                m_cachedMecanimBoneNameMap.Add("Spine", assetName + "_Spine");
                m_cachedMecanimBoneNameMap.Add("Chest", assetName + "_Spine1");
                m_cachedMecanimBoneNameMap.Add("Neck", assetName + "_Neck");
                m_cachedMecanimBoneNameMap.Add("Head", assetName + "_Head");

                m_cachedMecanimBoneNameMap.Add("LeftShoulder", assetName + "_LeftShoulder");
                m_cachedMecanimBoneNameMap.Add("LeftUpperArm", assetName + "_LeftArm");
                m_cachedMecanimBoneNameMap.Add("LeftLowerArm", assetName + "_LeftForeArm");
                m_cachedMecanimBoneNameMap.Add("LeftHand", assetName + "_LeftHand");

                m_cachedMecanimBoneNameMap.Add("RightShoulder", assetName + "_RightShoulder");
                m_cachedMecanimBoneNameMap.Add("RightUpperArm", assetName + "_RightArm");
                m_cachedMecanimBoneNameMap.Add("RightLowerArm", assetName + "_RightForeArm");
                m_cachedMecanimBoneNameMap.Add("RightHand", assetName + "_RightHand");

                m_cachedMecanimBoneNameMap.Add("LeftUpperLeg", assetName + "_LeftUpLeg");
                m_cachedMecanimBoneNameMap.Add("LeftLowerLeg", assetName + "_LeftLeg");
                m_cachedMecanimBoneNameMap.Add("LeftFoot", assetName + "_LeftFoot");
                m_cachedMecanimBoneNameMap.Add("LeftToeBase", assetName + "_LeftToeBase");

                m_cachedMecanimBoneNameMap.Add("RightUpperLeg", assetName + "_RightUpLeg");
                m_cachedMecanimBoneNameMap.Add("RightLowerLeg", assetName + "_RightLeg");
                m_cachedMecanimBoneNameMap.Add("RightFoot", assetName + "_RightFoot");
                m_cachedMecanimBoneNameMap.Add("RightToeBase", assetName + "_RightToeBase");


                m_cachedMecanimBoneNameMap.Add("Left Thumb Proximal", assetName + "_LeftHandThumb1");
                m_cachedMecanimBoneNameMap.Add("Left Thumb Intermediate", assetName + "_LeftHandThumb2");
                m_cachedMecanimBoneNameMap.Add("Left Thumb Distal", assetName + "_LeftHandThumb3");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Proximal", assetName + "_RightHandThumb1");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Intermediate", assetName + "_RightHandThumb2");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Distal", assetName + "_RightHandThumb3");

                m_cachedMecanimBoneNameMap.Add("Left Index Proximal", assetName + "_LeftHandIndex1");
                m_cachedMecanimBoneNameMap.Add("Left Index Intermediate", assetName + "_LeftHandIndex2");
                m_cachedMecanimBoneNameMap.Add("Left Index Distal", assetName + "_LeftHandIndex3");
                m_cachedMecanimBoneNameMap.Add("Right Index Proximal", assetName + "_RightHandIndex1");
                m_cachedMecanimBoneNameMap.Add("Right Index Intermediate", assetName + "_RightHandIndex2");
                m_cachedMecanimBoneNameMap.Add("Right Index Distal", assetName + "_RightHandIndex3");

                m_cachedMecanimBoneNameMap.Add("Left Middle Proximal", assetName + "_LeftHandMiddle1");
                m_cachedMecanimBoneNameMap.Add("Left Middle Intermediate", assetName + "_LeftHandMiddle2");
                m_cachedMecanimBoneNameMap.Add("Left Middle Distal", assetName + "_LeftHandMiddle3");
                m_cachedMecanimBoneNameMap.Add("Right Middle Proximal", assetName + "_RightHandMiddle1");
                m_cachedMecanimBoneNameMap.Add("Right Middle Intermediate", assetName + "_RightHandMiddle2");
                m_cachedMecanimBoneNameMap.Add("Right Middle Distal", assetName + "_RightHandMiddle3");

                m_cachedMecanimBoneNameMap.Add("Left Ring Proximal", assetName + "_LeftHandRing1");
                m_cachedMecanimBoneNameMap.Add("Left Ring Intermediate", assetName + "_LeftHandRing2");
                m_cachedMecanimBoneNameMap.Add("Left Ring Distal", assetName + "_LeftHandRing3");
                m_cachedMecanimBoneNameMap.Add("Right Ring Proximal", assetName + "_RightHandRing1");
                m_cachedMecanimBoneNameMap.Add("Right Ring Intermediate", assetName + "_RightHandRing2");
                m_cachedMecanimBoneNameMap.Add("Right Ring Distal", assetName + "_RightHandRing3");

                m_cachedMecanimBoneNameMap.Add("Left Little Proximal", assetName + "_LeftHandPinky1");
                m_cachedMecanimBoneNameMap.Add("Left Little Intermediate", assetName + "_LeftHandPinky2");
                m_cachedMecanimBoneNameMap.Add("Left Little Distal", assetName + "_LeftHandPinky3");
                m_cachedMecanimBoneNameMap.Add("Right Little Proximal", assetName + "_RightHandPinky1");
                m_cachedMecanimBoneNameMap.Add("Right Little Intermediate", assetName + "_RightHandPinky2");
                m_cachedMecanimBoneNameMap.Add("Right Little Distal", assetName + "_RightHandPinky3");
                break;
            case OptitrackBoneNameConvention.BVH:
                m_cachedMecanimBoneNameMap.Add("Hips", assetName + "_Hips");
                m_cachedMecanimBoneNameMap.Add("Spine", assetName + "_Chest");
                m_cachedMecanimBoneNameMap.Add("Chest", assetName + "_Chest2");
                m_cachedMecanimBoneNameMap.Add("Neck", assetName + "_Neck");
                m_cachedMecanimBoneNameMap.Add("Head", assetName + "_Head");

                m_cachedMecanimBoneNameMap.Add("LeftShoulder", assetName + "_LeftCollar");
                m_cachedMecanimBoneNameMap.Add("LeftUpperArm", assetName + "_LeftShoulder");
                m_cachedMecanimBoneNameMap.Add("LeftLowerArm", assetName + "_LeftElbow");
                m_cachedMecanimBoneNameMap.Add("LeftHand", assetName + "_LeftWrist");

                m_cachedMecanimBoneNameMap.Add("RightShoulder", assetName + "_RightCollar");
                m_cachedMecanimBoneNameMap.Add("RightUpperArm", assetName + "_RightShoulder");
                m_cachedMecanimBoneNameMap.Add("RightLowerArm", assetName + "_RightElbow");
                m_cachedMecanimBoneNameMap.Add("RightHand", assetName + "_RightWrist");

                m_cachedMecanimBoneNameMap.Add("LeftUpperLeg", assetName + "_LeftHip");
                m_cachedMecanimBoneNameMap.Add("LeftLowerLeg", assetName + "_LeftKnee");
                m_cachedMecanimBoneNameMap.Add("LeftFoot", assetName + "_LeftAnkle");
                m_cachedMecanimBoneNameMap.Add("LeftToeBase", assetName + "_LeftToe");

                m_cachedMecanimBoneNameMap.Add("RightUpperLeg", assetName + "_RightHip");
                m_cachedMecanimBoneNameMap.Add("RightLowerLeg", assetName + "_RightKnee");
                m_cachedMecanimBoneNameMap.Add("RightFoot", assetName + "_RightAnkle");
                m_cachedMecanimBoneNameMap.Add("RightToeBase", assetName + "_RightToe");


                m_cachedMecanimBoneNameMap.Add("Left Thumb Proximal", assetName + "_LeftFinger0");
                m_cachedMecanimBoneNameMap.Add("Left Thumb Intermediate", assetName + "_LeftFinger01");
                m_cachedMecanimBoneNameMap.Add("Left Thumb Distal", assetName + "_LeftFinger02");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Proximal", assetName + "_RightFinger0");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Intermediate", assetName + "_RightFinger01");
                m_cachedMecanimBoneNameMap.Add("Right Thumb Distal", assetName + "_RightFinger02");

                m_cachedMecanimBoneNameMap.Add("Left Index Proximal", assetName + "_LeftFinger1");
                m_cachedMecanimBoneNameMap.Add("Left Index Intermediate", assetName + "_LeftFinger11");
                m_cachedMecanimBoneNameMap.Add("Left Index Distal", assetName + "_LeftFinger12");
                m_cachedMecanimBoneNameMap.Add("Right Index Proximal", assetName + "_RightFinger1");
                m_cachedMecanimBoneNameMap.Add("Right Index Intermediate", assetName + "_RightFinger11");
                m_cachedMecanimBoneNameMap.Add("Right Index Distal", assetName + "_RightFinger12");

                m_cachedMecanimBoneNameMap.Add("Left Middle Proximal", assetName + "_LeftFinger2");
                m_cachedMecanimBoneNameMap.Add("Left Middle Intermediate", assetName + "_LeftFinger21");
                m_cachedMecanimBoneNameMap.Add("Left Middle Distal", assetName + "_LeftFinger22");
                m_cachedMecanimBoneNameMap.Add("Right Middle Proximal", assetName + "_RightFinger2");
                m_cachedMecanimBoneNameMap.Add("Right Middle Intermediate", assetName + "_RightFinger21");
                m_cachedMecanimBoneNameMap.Add("Right Middle Distal", assetName + "_RightFinger22");

                m_cachedMecanimBoneNameMap.Add("Left Ring Proximal", assetName + "_LeftFinger3");
                m_cachedMecanimBoneNameMap.Add("Left Ring Intermediate", assetName + "_LeftFinger31");
                m_cachedMecanimBoneNameMap.Add("Left Ring Distal", assetName + "_LeftFinger32");
                m_cachedMecanimBoneNameMap.Add("Right Ring Proximal", assetName + "_RightFinger3");
                m_cachedMecanimBoneNameMap.Add("Right Ring Intermediate", assetName + "_RightFinger31");
                m_cachedMecanimBoneNameMap.Add("Right Ring Distal", assetName + "_RightFinger32");

                m_cachedMecanimBoneNameMap.Add("Left Little Proximal", assetName + "_LeftFinger4");
                m_cachedMecanimBoneNameMap.Add("Left Little Intermediate", assetName + "_LeftFinger41");
                m_cachedMecanimBoneNameMap.Add("Left Little Distal", assetName + "_LeftFinger42");
                m_cachedMecanimBoneNameMap.Add("Right Little Proximal", assetName + "_RightFinger4");
                m_cachedMecanimBoneNameMap.Add("Right Little Intermediate", assetName + "_RightFinger41");
                m_cachedMecanimBoneNameMap.Add("Right Little Distal", assetName + "_RightFinger42");
                break;
        }
    }
}
