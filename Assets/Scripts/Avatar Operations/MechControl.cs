using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Mecanim Notes

//Apparently mecanim calls joints bones and bones muscles OKAY;
//Get internal returns world position while set requires local position in HumanPose.position

#endregion

public class MechControl : MonoBehaviour
{
    public Avatar DestinationAvatar;
    [Range(-1.0f,1.0f)]
    public float slider;
    [Range(0,94)]
    public int id_muscle;

    public int bone_index;

    public List<string> bone_names;
    public List<string> muscle_names;

    private HumanPose m_humanPose = new HumanPose();
    //Root of the skeleton hierarchy
    private GameObject m_rootObject;

    public Transform to_move;
    public Vector3 movement;

    private List<HumanBone> m_humanBonesList;
    private List<SkeletonBone> m_skeletonBonesList;
    Dictionary<int, GameObject> m_BoneMap;

    //Read poses
    private HumanPoseHandler m_srcPoseHandler;
    //Write Poses
    private HumanPoseHandler m_destPoseHandler;

    //Objective of this method is setting up the m_srcPoseHandler and m_destPoseHandler objects
    void MechanimSetup(string rootObj)
    {
        // Now set up the HumanDescription for the retargeting source Avatar.
        HumanDescription humanDesc = new HumanDescription();
        humanDesc.human = m_humanBonesList.ToArray();
        humanDesc.skeleton = m_skeletonBonesList.ToArray();

        // These all correspond to default values.
        humanDesc.upperArmTwist = 0.5f;
        humanDesc.lowerArmTwist = 0.5f;
        humanDesc.upperLegTwist = 0.5f;
        humanDesc.lowerLegTwist = 0.5f;
        humanDesc.armStretch = 0.05f;
        humanDesc.legStretch = 0.05f;
        humanDesc.feetSpacing = 0.0f;
        humanDesc.hasTranslationDoF = false;

        // Finally, take the description and build the Avatar and pose handlers.
        Avatar m_srcAvatar = AvatarBuilder.BuildHumanAvatar(m_rootObject, humanDesc);

        if (m_srcAvatar.isValid == false || m_srcAvatar.isHuman == false)
        {
            Debug.LogError(GetType().FullName + ": Unable to create source Avatar for retargeting. Check that your Skeleton Asset Name and Bone Naming Convention are configured correctly.", this);
            this.enabled = false;
            return;
        }

        m_srcPoseHandler  = new HumanPoseHandler(m_srcAvatar, m_rootObject.transform);
        m_destPoseHandler = new HumanPoseHandler(DestinationAvatar, this.transform);
    }

    int LookUpBone(string name)
    {
        for (int i = 0; i < HumanTrait.BoneName.Length; i++)
        {
            if (HumanTrait.BoneName[i] == name)
                return i;
        }

        return -1;
    }

    void CopyAvatar()
    {
        m_rootObject = new GameObject("Avatar Skeleton Copy");
        m_humanBonesList = new List<HumanBone>(DestinationAvatar.humanDescription.human.Length);
        m_skeletonBonesList = new List<SkeletonBone>(DestinationAvatar.humanDescription.human.Length + 1);

        {
            SkeletonBone rootBone = new SkeletonBone();
            rootBone.name = m_rootObject.name;
            rootBone.position = Vector3.zero;
            rootBone.rotation = Quaternion.identity;
            rootBone.scale = Vector3.one;

            m_skeletonBonesList.Add(rootBone);
        }

        m_BoneMap = new Dictionary<int, GameObject>();

        for (int bi = 0; bi < DestinationAvatar.humanDescription.human.Length; bi++)
        {
            HumanBone hBone = new HumanBone();
            HumanBone bone = DestinationAvatar.humanDescription.human[bi];
            string name = bone.boneName;
            int index = LookUpBone(bone.humanName);

            hBone.boneName = bone.boneName;
            hBone.humanName = bone.humanName;
            hBone.limit.useDefaultValues = true;
            m_humanBonesList.Add(hBone);

            GameObject child = new GameObject(name);
            m_BoneMap[index] = child;
        }

        foreach (SkeletonBone bone in DestinationAvatar.humanDescription.skeleton)
        {
            SkeletonBone newBone = new SkeletonBone();
            newBone.name = bone.name;
            newBone.position = bone.position;
            newBone.rotation = bone.rotation;
            newBone.scale = bone.scale;

            m_skeletonBonesList.Add(newBone);
        }

        foreach (KeyValuePair<int, GameObject> bone in m_BoneMap)
        {
            int parent_index = HumanTrait.GetParentBone(bone.Key);
            FromToLine deb = bone.Value.AddComponent<FromToLine>();
            bone.Value.transform.parent = parent_index == -1 ? m_rootObject.transform : m_BoneMap[parent_index].transform;
            bone.Value.transform.localPosition = Vector3.zero;
            bone.Value.transform.localRotation = Quaternion.identity;
            deb.target = parent_index == -1 ? m_rootObject.transform : m_BoneMap[parent_index].transform;
        }
    }

    void PositionBones()
    {
        foreach (KeyValuePair<int, GameObject> bone in m_BoneMap)
        {
            foreach (SkeletonBone refBone in DestinationAvatar.humanDescription.skeleton)
            {
                if (refBone.name == bone.Value.name)
                {
                    bone.Value.transform.localPosition = refBone.position;
                    bone.Value.transform.localRotation = refBone.rotation;
                }
            }
        }
    }

    [ContextMenu("Start")]
    private void Start()
    {

        CopyAvatar();

        MechanimSetup("root_bone");

        m_rootObject.transform.localPosition = Vector3.zero;
        m_rootObject.transform.localRotation = Quaternion.identity;

        PositionBones();

        bone_names = new List<string>(HumanTrait.BoneCount);
        muscle_names = new List<string>(HumanTrait.MuscleCount);

        foreach (string str in HumanTrait.BoneName)
        {
            bone_names.Add(str);
        }

        foreach (string str in HumanTrait.MuscleName)
        {
            muscle_names.Add(str);
        }
    }

    [ContextMenu("Get Stuff")]
    void getStuff()
    {
        int muscle_a = HumanTrait.MuscleFromBone(bone_index, 0);
        int muscle_b = HumanTrait.MuscleFromBone(bone_index, 1);
        int muscle_c = HumanTrait.MuscleFromBone(bone_index, 2);

        Debug.Log("Muscle A: " + muscle_a.ToString(), this);
        Debug.Log("Muscle B: " + muscle_b.ToString(), this);
        Debug.Log("Muscle C: " + muscle_c.ToString(), this);
    }

    private void Update()
    {
        // Modify Positions
        {

        }

        // Perform Mecanim retargeting.
        if (m_srcPoseHandler != null && m_destPoseHandler != null)
        {
            // Interpret the streamed pose into Mecanim muscle space representation.
            m_srcPoseHandler.GetHumanPose(ref m_humanPose);

            // Re-target that muscle space pose to the destination avatar.
            m_destPoseHandler.SetHumanPose(ref m_humanPose);
        }
    }
}
