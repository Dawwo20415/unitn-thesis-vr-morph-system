

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarPipeline : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("By default an Animator component will be searched on the GameObject")]
    public Avatar destination_avatar;
    public Transform skeleton_root;

    [Header("Operations")]
    [Tooltip("This objects need to have the \"AvatarOperation\" or derived component")]
    public List<GameObject> operations_obj;
    private List<AvatarOperation> operations;

    [Header("Debug")]
    public bool add_lines_to_mock_avatar;

    private GameObject m_root_obj;
    private List<HumanBone> m_human_bones_list;
    private List<SkeletonBone> m_skeleton_bones_list;
    private Dictionary<int, GameObject> m_bone_map;

    private HumanPoseHandler m_src_pose_handler;
    private HumanPoseHandler m_dest_pose_handler;
    private HumanPose m_human_pose = new HumanPose();

    struct Point
    {
        Vector3 position;
        Quaternion orientation;
    }

    /*
     * 0. Creation of Alternative Avatar by Optitrack proportions
     * 1. Setting point positions from optitrack (base)
     * 2. Modifying point positions from egocentric body mapping
     * 3. Modifying point positions from point displacement addon
     * 
     * At a certain point there is the need to recalculate the IK of arms and legs
     * Mecanim works only with the orientations and then the length of segments is fixed, even if you
     * change positions of points it is of no impact on the final Avatar Position
     * 
     * IK should be recalculated at the end of the whole process
     * 
     * Using the optitrack position as baseline we can choose the transformation that is nearest to the original
     * 
     * Points like the elbow need to be classified as derivative? Like they are the ones in which both position and orientation needs to be recalculated
     * Meanwhile some points you set position and need to only ricalculate the orientation
     * For some you don't need to calcuate either, like the hands. 
     * 
     * This pipeline needs to be like agnostic to the starting avatar
     *  Probably will need a specification to know it it is starting and ending on the same avatar because
     *  of that whole thing about local coordinates, and whatnot
     *  
     * TODO: re-index the bones based on the skeleton rather than the Human Trait, or make a translator between them
     */

    private void OnEnable()
    {
        foreach (GameObject obj in operations_obj)
        {
            operations.Add(obj.GetComponent<AvatarOperation>());
        }
    }

    private void Start()
    {
        GenerateAvatarCopy();

        MechanimSetup(m_root_obj.name);

        m_root_obj.transform.localPosition = Vector3.zero;
        m_root_obj.transform.localRotation = Quaternion.identity;

        PositionBones();
    }

    private void Update()
    {
        if (m_src_pose_handler != null && m_dest_pose_handler != null)
        {
            // Interpret the streamed pose into Mecanim muscle space representation.
            m_src_pose_handler.GetHumanPose(ref m_human_pose);

            // Re-target that muscle space pose to the destination avatar.
            m_dest_pose_handler.SetHumanPose(ref m_human_pose);
        }
    }

    #region private methods

    void MechanimSetup(string rootObj)
    {
        // Now set up the HumanDescription for the retargeting source Avatar.
        HumanDescription humanDesc = new HumanDescription();
        humanDesc.human = m_human_bones_list.ToArray();
        humanDesc.skeleton = m_skeleton_bones_list.ToArray();

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
        Avatar m_srcAvatar = AvatarBuilder.BuildHumanAvatar(m_root_obj, humanDesc);

        if (m_srcAvatar.isValid == false || m_srcAvatar.isHuman == false)
        {
            Debug.LogError(GetType().FullName + ": Unable to create source Avatar for retargeting. Check that your Skeleton Asset Name and Bone Naming Convention are configured correctly.", this);
            this.enabled = false;
            return;
        }

        m_src_pose_handler = new HumanPoseHandler(m_srcAvatar, m_root_obj.transform);
        m_dest_pose_handler = new HumanPoseHandler(destination_avatar, this.transform);
    }

    void GenerateAvatarCopy()
    {
        m_root_obj = new GameObject(destination_avatar.name + " - Alternate");
        m_human_bones_list = new List<HumanBone>(destination_avatar.humanDescription.human.Length);
        m_skeleton_bones_list = new List<SkeletonBone>(destination_avatar.humanDescription.human.Length + 1);
        m_bone_map = new Dictionary<int, GameObject>(destination_avatar.humanDescription.human.Length);

        CreateHumanDefinition();
        CreateSkeletonDefinition();

        //Parent & Move Object in the default T-Pose
        foreach (KeyValuePair<int, GameObject> bone in m_bone_map)
        {
            int parent_index = HumanTrait.GetParentBone(bone.Key);
            bone.Value.transform.parent = parent_index == -1 ? m_root_obj.transform : m_bone_map[parent_index].transform;
            bone.Value.transform.localPosition = Vector3.zero;
            bone.Value.transform.localRotation = Quaternion.identity;

            if (add_lines_to_mock_avatar)
            {
                FromToLine deb = bone.Value.AddComponent<FromToLine>();
                deb.target = parent_index == -1 ? m_root_obj.transform : m_bone_map[parent_index].transform;
            }
        }
    }

    void CreateSkeletonDefinition()
    {
        { //Special case for root skeleton bone
            SkeletonBone root_bone = new SkeletonBone();
            root_bone.name = m_root_obj.name;
            root_bone.position = Vector3.zero;
            root_bone.rotation = Quaternion.identity;
            root_bone.scale = Vector3.one;

            m_skeleton_bones_list.Add(root_bone);
        }

        foreach (SkeletonBone bone in destination_avatar.humanDescription.skeleton)
        {
            SkeletonBone newBone = new SkeletonBone();
            newBone.name = bone.name;
            newBone.position = bone.position;
            newBone.rotation = bone.rotation;
            newBone.scale = bone.scale;

            m_skeleton_bones_list.Add(newBone);
        }
    }

    void CreateHumanDefinition()
    {
        for (int bi = 0; bi < destination_avatar.humanDescription.human.Length; bi++)
        {
            HumanBone hBone = new HumanBone();
            HumanBone bone = destination_avatar.humanDescription.human[bi];
            string name = bone.boneName;
            int index = LookUpBone(bone.humanName);

            hBone.boneName = bone.boneName;
            hBone.humanName = bone.humanName;
            hBone.limit.useDefaultValues = true;
            m_human_bones_list.Add(hBone);

            GameObject child = new GameObject(name);
            m_bone_map[index] = child;
        }
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

    void PositionBones()
    {
        foreach (KeyValuePair<int, GameObject> bone in m_bone_map)
        {
            foreach (SkeletonBone refBone in destination_avatar.humanDescription.skeleton)
            {
                if (refBone.name == bone.Value.name)
                {
                    bone.Value.transform.localPosition = refBone.position;
                    bone.Value.transform.localRotation = refBone.rotation;
                }
            }
        }
    }

    #endregion

}
