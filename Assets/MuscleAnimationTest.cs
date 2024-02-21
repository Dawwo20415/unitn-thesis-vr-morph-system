using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuscleAnimationTest : MonoBehaviour
{
    public Transform root_obj;
    [Header("Controll")]
    [Range(0,52)]
    public int limb_id;
    [Range(0,2)]
    public int axis;
    [Range(-1.0f, 1.0f)]
    public float modifierx;
    [Range(-1.0f, 1.0f)]
    public float modifiery;
    [Range(-1.0f, 1.0f)]
    public float modifierz;
    public bool modif;

    public GameObject root;
    public Transform temp_joint;
    private Animator c_animator;
    private Avatar avatar;
    private HumanPose m_human_pose = new HumanPose();
    private HumanPoseHandler m_destPoseHandler;
    private int idx;
    private int idy;
    private int idz;

    private float muscle_position_1;
    private float muscle_position_2;
    private float muscle_position_3;

    // Start is called before the first frame update
    void Start()
    {
        c_animator = GetComponent<Animator>();
        avatar = c_animator.avatar;
        m_destPoseHandler = new HumanPoseHandler(avatar, root_obj);
        m_destPoseHandler.GetHumanPose(ref m_human_pose);
        int id = HumanTrait.MuscleFromBone(limb_id, axis);

        idx = HumanTrait.MuscleFromBone(limb_id, 0);
        idy = HumanTrait.MuscleFromBone(limb_id, 1);
        idz = HumanTrait.MuscleFromBone(limb_id, 2);
        muscle_position_1 = m_human_pose.muscles[idx];
        muscle_position_2 = m_human_pose.muscles[idy];
        muscle_position_3 = m_human_pose.muscles[idz];

        Debug.Log("1[" + muscle_position_1 + "] 2[" + muscle_position_2 + "] 3[" + muscle_position_3 + "]", this);

        /*
        bool def = avatar.humanDescription.human[limb_id].limit.useDefaultValues; 
        Vector3 max;
        Vector3 min;

        if (def)
        {
            max = new Vector3(HumanTrait.GetMuscleDefaultMax(idx), HumanTrait.GetMuscleDefaultMax(idy), HumanTrait.GetMuscleDefaultMax(idz));
            min = new Vector3(HumanTrait.GetMuscleDefaultMin(idx), HumanTrait.GetMuscleDefaultMin(idy), HumanTrait.GetMuscleDefaultMin(idz));
        } else
        {
            max = avatar.humanDescription.human[limb_id].limit.max;
            min = avatar.humanDescription.human[limb_id].limit.min;
        }
        //Debug.Log("Max[" + max + "] Min[" + min + "] - Center[" + center + "]", this);

        Vector3 muscle = new Vector3(m_human_pose.muscles[idx], m_human_pose.muscles[idy], m_human_pose.muscles[idz]);
        Vector3 worldRot = c_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).rotation.eulerAngles;
        Vector3 localRot = c_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).localRotation.eulerAngles;
        //Debug.Log("Initial | Muscle[" + muscle + "] - WorldEuler[" + worldRot + "] - LocalEuler[" + localRot + "]", this);
        */
    }

    // Update is called once per frame
    void Update()
    {
        DirectKinematic(ref m_human_pose);
    }

    private void LateUpdate()
    {
        //ManualSetHumanPose(ref m_human_pose);
        //m_destPoseHandler.SetHumanPose(ref m_human_pose);
        //obj.transform.position = m_human_pose.bodyPosition * c_animator.humanScale;

    }

    private void DirectKinematic(ref HumanPose pose)
    {

        Vector3 muscle = new Vector3(m_human_pose.muscles[idx], m_human_pose.muscles[idy], m_human_pose.muscles[idz]);
        Vector3 worldRot = c_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).rotation.eulerAngles;
        Vector3 localRot = c_animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).localRotation.eulerAngles;
        //Debug.Log("Muscle[" +  muscle + "] - WorldEuler[" + worldRot + "] - LocalEuler[" + localRot + "]", this);
        
        
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Quaternion center = Quaternion.Euler(-0.74f, 44.504f, 126.213f);
        bool def = avatar.humanDescription.human[limb_id].limit.useDefaultValues;
        Vector3 max;
        Vector3 min;

        if (def)
        {
            max = new Vector3(HumanTrait.GetMuscleDefaultMax(idx), HumanTrait.GetMuscleDefaultMax(idy), HumanTrait.GetMuscleDefaultMax(idz));
            min = new Vector3(HumanTrait.GetMuscleDefaultMin(idx), HumanTrait.GetMuscleDefaultMin(idy), HumanTrait.GetMuscleDefaultMin(idz));
        }
        else
        {
            max = avatar.humanDescription.human[limb_id].limit.max;
            min = avatar.humanDescription.human[limb_id].limit.min;
        }

        float x_angle, y_angle, z_angle;

        if (modif)
        {
            x_angle = map0(modifierx, -1.0f, 1.0f, min.z, max.z);
            //Debug.Log("X_Angle - " + modifierx + " " + min.z + " " + max.z + " " + x_angle, this);
            y_angle = map0(modifiery, -1.0f, 1.0f, min.y, max.y);
            //Debug.Log("Y_Angle - " + modifiery + " " + min.y + " " + max.y + " " + y_angle, this);
            z_angle = map0(modifierz, -1.0f, 1.0f, min.x, max.x);
            //Debug.Log("Z_Angle - " + modifierz + " " + min.x + " " + max.x + " " + z_angle, this);
        }
        else
        {
            x_angle = map0(muscle_position_3, -1.0f, 1.0f, min.z, max.z);
            //Debug.Log("X_Angle - " + modifierx + " " + min.z + " " + max.z + " " + x_angle, this);
            y_angle = map0(muscle_position_2, -1.0f, 1.0f, min.y, max.y);
            //Debug.Log("Y_Angle - " + modifiery + " " + min.y + " " + max.y + " " + y_angle, this);
            z_angle = map0(muscle_position_1, -1.0f, 1.0f, min.x, max.x);
            //Debug.Log("Z_Angle - " + modifierz + " " + min.x + " " + max.x + " " + z_angle, this);
        }

        Quaternion rotation = Quaternion.AngleAxis(x_angle, Vector3.back) * Quaternion.AngleAxis(y_angle, Vector3.right) * Quaternion.AngleAxis(z_angle, Vector3.up);

        c_animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, center * rotation);
    }

    float map0(float x, float in_min, float in_max, float out_min, float out_max)
    {
        if (x > 0.0f)
        {
            return map(x, 0.0f, in_max, 0.0f, out_max);
        } else
        {
            return map(x, in_min, 0.0f, out_min, 0.0f);
        }
    }

    float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
