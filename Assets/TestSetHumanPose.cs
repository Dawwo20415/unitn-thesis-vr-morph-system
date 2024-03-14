using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSetHumanPose : MonoBehaviour
{
    public int bone;
    public Transform root;
    public Avatar avatar;
    [Range(-1.0f, 1.0f)]
    public float modify_x;
    [Range(-1.0f, 1.0f)]
    public float modify_y;
    [Range(-1.0f, 1.0f)]
    public float modify_z;
    public Transform hips;

    private Animator c_animator;
    //private Avatar avatar;
    private HumanPose pose = new HumanPose();
    private HumanPoseHandler poseHandler;
    private Animator animator;
    [SerializeField]
    private int idx, idy, idz;
    [SerializeField]
    private float value;
    private Vector3 hipsOffset;
    private Vector3 postHips;

    void Start()
    {
        animator = GetComponent<Animator>();
        //avatar = c_animator.avatar;

        poseHandler = new HumanPoseHandler(avatar, root);
        poseHandler.GetHumanPose(ref pose);

        idx = HumanTrait.MuscleFromBone(bone, 0);
        idy = HumanTrait.MuscleFromBone(bone, 1);
        idz = HumanTrait.MuscleFromBone(bone, 2);

        value = pose.muscles[idx];

        
        for (int i = 0; i < pose.muscles.Length; i++)
        {
            pose.muscles[i] = 0;
        }

        poseHandler.SetHumanPose(ref pose);
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (idx != -1)
            pose.muscles[idx] = modify_x;


        if (idy != -1)
            pose.muscles[idy] = modify_y;

        if (idz != -1)
            pose.muscles[idz] = modify_z;


    }

    private void LateUpdate()
    {
        poseHandler.SetHumanPose(ref pose);
    }

    private Vector3 calculateCenterOfMass()
    {
        
        
        Vector3 center = Vector3.zero;

        return center;
    }
}
