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
    [Range(-2.0f, 2.0f)]
    public float modifier;

    private Animator c_animator;
    private Avatar avatar;
    private HumanPose m_human_pose = new HumanPose();
    private HumanPoseHandler m_destPoseHandler;
    private float value;

    // Start is called before the first frame update
    void Start()
    {
        c_animator = GetComponent<Animator>();
        avatar = c_animator.avatar;
        m_destPoseHandler = new HumanPoseHandler(avatar, root_obj);
        m_destPoseHandler.GetHumanPose(ref m_human_pose);

        int id = HumanTrait.MuscleFromBone(limb_id, axis);
        value = m_human_pose.muscles[id];
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("0 - " + m_human_pose.bodyPosition.ToString(), this);

        DirectKinematic(ref m_human_pose);

        Debug.Log("1 - " + m_human_pose.bodyPosition.ToString(), this);

        

        Debug.Log("2 - " + m_human_pose.bodyPosition.ToString(), this);
    }

    private void LateUpdate()
    {
        m_destPoseHandler.SetHumanPose(ref m_human_pose);
    }

    private void DirectKinematic(ref HumanPose pose)
    {
        int id = HumanTrait.MuscleFromBone(limb_id, axis);
        m_human_pose.muscles[id] = value + modifier;
    }

}
