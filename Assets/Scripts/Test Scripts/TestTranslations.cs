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
            Debug.Log("HumanBone Index[" + i + "] Human_name[" + hd.human[i].humanName + "] Skeleton_name[" + hd.human[i].boneName + "]", this);
        }

        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            Debug.Log("SkeletonBone Index[" + i + "] skeleton_name[" + hd.skeleton[i].name + "]", this);
        }

        Dictionary<int, int> translation = MecanimHumanoidExtension.HD_skeleton2human(hd);

        foreach ((int a, int b) in translation)
        {
            Debug.Log("A[" + a + "] B[" + b + "]", this);
        }

    }

}
