using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LIST OF INDEX SPACES
/// * HumanBodyBones
/// * HumanDescription Human array index
/// * HumanDescription Skeleton array index
/// * Optitrack IDs
/// * AnimationHumanStream MuscleHandle's 
/// * Individual Playables Indexes
/// * HumanTrait
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
