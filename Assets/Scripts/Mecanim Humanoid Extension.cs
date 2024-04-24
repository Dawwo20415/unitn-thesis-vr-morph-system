using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LIST OF INDEX SPACES 
/// * HumanBodyBones                        |
/// * HumanDescription Human array index    | X
/// * HumanDescription Skeleton array index | X
/// * Optitrack IDs                         |
/// * AnimationHumanStream MuscleHandle's   |
/// * Individual Playables Indexes          |
/// * HumanTrait                            | X
/// </summary>

public static class MecanimHumanoidExtension
{
    public static bool debugFlag = true;

    ///<summary> Key: HumanDescription - human | Value: HumanDescription - skeleton</summary>
    public static Dictionary<int,int> HD_human2skeleton (HumanDescription hd)
    {
        Dictionary<int, int> translation = new Dictionary<int, int>(hd.human.Length);

        for (int i = 0; i < hd.human.Length; i++)
        {
            translation[i] = LookUpSkeleton(hd.human[i].boneName, hd);
        }

        return translation;
    }

    ///<summary> Key: HumanDescription - skeleton | Value: HumanDescription - human</summary>
    public static Dictionary<int, int> HD_skeleton2human (HumanDescription hd)
    {
        Dictionary<int, int> translation = new Dictionary<int, int>(hd.human.Length);

        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            translation[i] = LookUpHumanBone(hd.skeleton[i].name, hd);
        }

        return translation;
    }

    ///<summary> Key: HumanTrait - BoneName | Value: HumanDescription - human</summary>
    public static Dictionary<int, int> HumanTrait2HumanDescription_human (HumanDescription hd)
    {
        Dictionary<int, int> translation = new Dictionary<int, int>(HumanTrait.BoneCount);

        for (int i = 0; i < HumanTrait.BoneCount; i++)
        {
            translation[i] = LookUpHumanHuman(HumanTrait.BoneName[i], hd);
        }

        return translation;
    }

    ///<summary> Key: HumanTrait - BoneName | Value: HumanDescription - human</summary>
    public static Dictionary<int, int> HumanBodyBones2HumanDescription_human(HumanDescription hd)
    {
        Dictionary<int, int> translation = new Dictionary<int, int>((int)HumanBodyBones.LastBone);

        //Names for the Hand bones are with a " " (space) in the HumanDescription side
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            string name = System.Enum.GetName(typeof(HumanBodyBones), i);
            int j = LookUpHumanHuman(name, hd);
            translation[i] = j;
            if (j != -1)
            {
                Debug.Log("Translation: HumanBodyBones [" + name + "," + i + "] corresponds to HumanDescription [" + hd.human[j].humanName + "," + j + "]");
            } else
            {
                Debug.Log("Translation: HumanBodyBones [" + name + "," + i + "] does not correspond to anything");
            }
        }

        return translation;
    }

    public static Dictionary<int, int> OptitrackId2HumanBodyBones(Dictionary<int, GameObject> guide, Animator animator)
    {
        Dictionary<int, int> translation = new Dictionary<int, int>(guide.Count);

        foreach ((int key, GameObject obj) in guide)
        {
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                if (animator.GetBoneTransform((HumanBodyBones)i) == obj.transform)
                {
                    translation[key] = i;
                    break;
                }
            }
        }

        return translation;
    }

    private static int LookUpHumanBone(string name, HumanDescription hd)
    {
        for (int i = 0; i < hd.human.Length; i++)
        {
            if (name == hd.human[i].boneName)
                return i;
        }

        if (debugFlag) { Debug.Log("Failed to find HumanBone from SkeletonBone named: " + name + " | For HumanDescription: " + hd.ToString()); }
        return -1;
    }

    private static int LookUpHumanHuman(string name, HumanDescription hd)
    {
        for (int i = 0; i < hd.human.Length; i++)
        {
            if (name == hd.human[i].humanName)
                return i;
        }

        if (debugFlag) { Debug.Log("Failed to find HumanBone from SkeletonBone named: " + name + " | For HumanDescription: " + hd.ToString()); }
        return -1;
    }

    private static int LookUpSkeleton(string name, HumanDescription hd)
    {
        for (int i = 0; i < hd.skeleton.Length; i++)
        {
            if (name == hd.skeleton[i].name)
                return i;
        }

        if (debugFlag) { Debug.Log("Failed to find SkeletonBone from HumanBone named: " + name + " | For HumanDescription: " + hd.ToString()); }
        return -1;
    }

}
