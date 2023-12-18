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
    [SerializeField]
    private List<AvatarOperation> operations;

    [Header("Debug")]
    public bool add_lines_to_mock_avatar;

    private GameObject m_root_obj;
    private List<HumanBone> m_human_bones_list;
    private List<SkeletonBone> m_skeleton_bones_list;
    private Dictionary<int, GameObject> m_bone_map;

    private HumanPose m_human_pose = new HumanPose();
    private HumanPoseHandler m_destPoseHandler;

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
        operations = new List<AvatarOperation>();

        AvatarOperation[] own_op = this.GetComponents<AvatarOperation>();
        if (own_op.Length > 0)
        {
            foreach (AvatarOperation op in own_op)
            {
                if (op.isActiveAndEnabled)
                    operations.Add(op);
            }
        }

        foreach (GameObject obj in operations_obj)
        {
            if (obj.GetComponent<AvatarOperation>())
                operations.Add(obj.GetComponent<AvatarOperation>());
        }
    }

    private void Start()
    {
        m_destPoseHandler = new HumanPoseHandler(destination_avatar, skeleton_root);

        m_bone_map = new Dictionary<int, GameObject>(destination_avatar.humanDescription.human.Length);

        mapObjects2Bones();

        m_destPoseHandler.GetHumanPose(ref m_human_pose);
    }

    private void Update()
    {
        //Compute cycle

        foreach (AvatarOperation op in operations)
        {
            if (op.isActiveAndEnabled)
                op.Compute(m_bone_map, ref m_human_pose);
        }

        RecalculateIK();

        m_destPoseHandler.SetHumanPose(ref m_human_pose);
    }

    #region private methods

    private void RecalculateIK()
    {
        //TODO
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

    private void mapObjects2Bones()
    {
        Transform[] childs = GetComponentsInChildren<Transform>();
        foreach (Transform child in childs)
        {
            int index = -1;

            foreach (HumanBone hb in destination_avatar.humanDescription.human)
            {
                if (hb.boneName == child.name)
                {
                    index = LookUpBone(hb.humanName);
                    break;
                }
            }

            if (index != -1)
                m_bone_map[index] = child.gameObject;
        }
    }

    #endregion

}
