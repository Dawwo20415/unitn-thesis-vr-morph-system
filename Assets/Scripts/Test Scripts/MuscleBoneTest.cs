using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuscleBoneTest : MonoBehaviour
{
    [Range(0,54)]
    public int bone;
    public Vector3 bodyPosition;
    public Quaternion bodyRotation;
    [Range(-1.0f, 1.0f)]
    public float modify_x;
    [Range(-1.0f, 1.0f)]
    public float modify_y;
    [Range(-1.0f, 1.0f)]
    public float modify_z;
    public HumanoidAvatarDefaults centers;
    public MechanimBoneMotionMapping mapping;

    [Header("Private Peek")]
    public bool useless;
    private Animator c_animator;
    private Avatar avatar;
    private Quaternion center;
    [SerializeField]
    private bool default_limits;
    [SerializeField]
    private int midx, midy, midz;
    [SerializeField]
    private Vector3 max, min;

    // Start is called before the first frame update
    void Start()
    {
        c_animator = GetComponent<Animator>();
        avatar = c_animator.avatar;
        default_limits = avatar.humanDescription.human[bone].limit.useDefaultValues;
        center = centers.rotations[bone];

        midx = HumanTrait.MuscleFromBone(bone, 0);
        midy = HumanTrait.MuscleFromBone(bone, 1);
        midz = HumanTrait.MuscleFromBone(bone, 2);

        Quaternion world = c_animator.GetBoneTransform((HumanBodyBones)bone).rotation;
        Quaternion local = c_animator.GetBoneTransform((HumanBodyBones)bone).localRotation;
        //orientation = Quaternion.Inverse(local) * world;

        string to_print = "Bone " + bone + " is " + HumanTrait.BoneName[bone] + 
            " | Muscles[(X/" + midx + "/" + (midx != -1 ? HumanTrait.MuscleName[midx] : "Not Available") + "),(Y/" + midy + "/" + (midy != -1 ? HumanTrait.MuscleName[midy] : "Not Available") + "),(Z/" + midz + "/" + (midz != -1 ? HumanTrait.MuscleName[midz] : "Not Available") + ")]";
        Debug.Log(to_print, this);

        to_print = "World[" + world.eulerAngles + "] local[" + local.eulerAngles + "]";
        Debug.Log(to_print, this);

        if (default_limits)
        {
            max = new Vector3(midx != -1 ? HumanTrait.GetMuscleDefaultMax(midx) : 0.0f, midy != -1 ? HumanTrait.GetMuscleDefaultMax(midy) : 0.0f, midz != -1 ? HumanTrait.GetMuscleDefaultMax(midz) : 0.0f);
            min = new Vector3(midx != -1 ? HumanTrait.GetMuscleDefaultMin(midx) : 0.0f, midy != -1 ? HumanTrait.GetMuscleDefaultMin(midy) : 0.0f, midz != -1 ? HumanTrait.GetMuscleDefaultMin(midz) : 0.0f);  
        }
        else
        {
            max = avatar.humanDescription.human[bone].limit.max;
            min = avatar.humanDescription.human[bone].limit.min;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        float x_angle, y_angle, z_angle;

        x_angle = map0(modify_x, -1.0f, 1.0f, min.x, max.x);
        y_angle = map0(modify_y, -1.0f, 1.0f, min.y, max.y);
        z_angle = map0(modify_z, -1.0f, 1.0f, min.z, max.z);

        Quaternion adjustment = mapping.mappings[bone];

        Quaternion rotation = Quaternion.AngleAxis(x_angle, adjustment * Vector3.up) * Quaternion.AngleAxis(y_angle, adjustment * Vector3.forward) * Quaternion.AngleAxis(z_angle, adjustment * Vector3.left);
        c_animator.SetBoneLocalRotation((HumanBodyBones)bone, center * rotation);
        c_animator.bodyPosition = bodyPosition;
        c_animator.bodyRotation = bodyRotation;
    }

    float map0(float x, float in_min, float in_max, float out_min, float out_max)
    {
        if (x > 0.0f)
        {
            return map(x, 0.0f, in_max, 0.0f, out_max);
        }
        else
        {
            return map(x, in_min, 0.0f, out_min, 0.0f);
        }
    }

    float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

}
