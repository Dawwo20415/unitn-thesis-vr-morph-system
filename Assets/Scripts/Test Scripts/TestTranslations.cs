using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TestTranslations : MonoBehaviour
{

    void Start()
    {
        Animator animator = GetComponent<Animator>();
        HumanDescription hd = animator.avatar.humanDescription;

        
        for (int i = 0; i < hd.human.Length; i++)
        {
            //Debug.Log("HumanBone Index[" + i + "] Human_name[" + hd.human[i].humanName + "] Skeleton_name[" + hd.human[i].boneName + "]", this);
        }

        //Debug.Log("Skeleton Bones", this);
        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            //Debug.Log("SkeletonBone Index[" + i + "] skeleton_name[" + hd.skeleton[i].name + "]", this);
        }

        List<Transform> bones = new List<Transform>();
        //Debug.Log("Animator GetBonesTransform", this);
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Transform trn = animator.GetBoneTransform((HumanBodyBones)i);
            if (trn)
            {
                bones.Add(trn);
                //Debug.Log("At index [" + i + "] the bone [" + trn.name + "] was found", this);
            }
        }

        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < bones.Count; j++)
            {
                if (hd.skeleton[i].name == bones[j].name)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.Log("Bone [" + hd.skeleton[i].name + "] has not been found in the animator");
            }
        }

    }

}
