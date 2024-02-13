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

    [Header("PlaceHolder")]
    public bool do_IK;
    public bool do_DK;
    [Range(0,100)]
    public int muscle_id;
    private float value;
    [Range(-20.0f,20.0f)]
    public float modifier = 0.0f;
    public List<string> IK_limb;
    public List<int> IK_limb_id;
    [Range(0,180)]
    public float manual_angle;
    public Vector3 manual_rotation;
    public Vector3 manual_rotation_2;
    private Transform tr0;
    private Transform tr1;
    private Transform tr2;
    private float length1;
    private float length2;
    

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

        tr0 = m_bone_map[IK_limb_id[0]].transform;
        tr1 = m_bone_map[IK_limb_id[1]].transform;
        tr2 = m_bone_map[IK_limb_id[2]].transform;
        length1 = tr1.localPosition.magnitude;
        length2 = tr2.localPosition.magnitude;

        int id = HumanTrait.MuscleFromBone(IK_limb_id[0], 1);
        value = m_human_pose.muscles[id];
    }

    private void LateUpdate()
    {
        //Compute cycle

        foreach (AvatarOperation op in operations)
        {
            if (op.isActiveAndEnabled)
                op.Compute(m_bone_map, ref m_human_pose);
        }

        if (do_IK)
            RecalculateIK();

        if (do_DK)
            DirectKinematic(ref m_human_pose);

        m_destPoseHandler.SetHumanPose(ref m_human_pose);
    }

    #region private methods

    private void DirectKinematic(ref HumanPose pose)
    {
        int id = HumanTrait.MuscleFromBone(IK_limb_id[0], 1);
        m_human_pose.muscles[id] = value + modifier;
    }

    private void RecalculateIK()
    {
        Quaternion direction = Quaternion.identity;
        Quaternion elbow_angle = Quaternion.identity;
        Quaternion shoulder_angle = Quaternion.identity;

        Vector3 x = tr0.rotation * Vector3.forward * length1;
        Vector3 y = tr0.rotation * Vector3.right * length1;
        Vector3 z = tr0.rotation * Vector3.up * length1;

        Debug.DrawLine(tr0.position, tr0.position + x, Color.blue, Time.deltaTime, false);
        Debug.DrawLine(tr0.position, tr0.position + y, Color.red, Time.deltaTime, false);
        Debug.DrawLine(tr0.position, tr0.position + z, Color.green, Time.deltaTime, false);

        Vector3 newUp = Vector3.Cross(tr2.position - tr1.position, tr0.position - tr1.position);
        Vector3 dir = (tr2.position - tr0.position).normalized;

        Quaternion boneOffset = Quaternion.Euler(90,0,0);
        direction = Quaternion.LookRotation(dir, z) * boneOffset;

        x = direction * Vector3.forward * length1;
        y = direction * Vector3.right * length1;
        z = direction * Vector3.up * length1;

        Debug.DrawLine(tr0.position, tr0.position + x, Color.blue, Time.deltaTime, false);
        Debug.DrawLine(tr0.position, tr0.position + y, Color.red, Time.deltaTime, false);
        Debug.DrawLine(tr0.position, tr0.position + z, Color.green, Time.deltaTime, false);

        //Calc Angle
        float angleA, angleB;
        float target_distance = (tr2.position - tr0.position).magnitude;
        if (target_distance >= (length1 + length2))
        {
            //Debug.DrawLine(tr0.position, tr2.position, Color.white, Time.deltaTime, false);
            angleA = 0.0f;
            angleB = 0.0f;
        }
        else
        {
            float fA = (target_distance * target_distance) + (length1 * length1) - (length2 * length2);
            float fB = (length1 * length1) + (length2 * length2) - (target_distance * target_distance);
            float fC = (2 * length1 * target_distance);
            float fD = (2 * length1 * length2);
            angleA = Mathf.Acos(fA / fC) * Mathf.Rad2Deg;
            angleB = Mathf.Acos(fB / fD) * Mathf.Rad2Deg;
            //Debug.DrawLine(tr0.position, tr2.position, Color.red, Time.deltaTime, false);
        }

        shoulder_angle = Quaternion.Euler(-angleA, 0, 0);
        elbow_angle *= direction * Quaternion.Euler(0, 0, 180 - angleA - angleB);

        x = direction * shoulder_angle * Vector3.forward * length1;
        y = direction * shoulder_angle * Vector3.right * length1;
        z = direction * shoulder_angle * Vector3.up * length1;

        Debug.DrawLine(tr0.position, tr0.position + x, Color.blue, Time.deltaTime, false);
        Debug.DrawLine(tr0.position, tr0.position + y, Color.red, Time.deltaTime, false);
        Debug.DrawLine(tr0.position, tr0.position + z, Color.green, Time.deltaTime, false);

        tr0.rotation = direction * shoulder_angle;
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
