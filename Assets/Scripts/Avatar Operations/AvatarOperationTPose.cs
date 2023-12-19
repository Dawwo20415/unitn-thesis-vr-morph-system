using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarOperationTPose : AvatarOperation
{
    public Avatar destination_avatar;

    public void Start()
    {
        
    }

    public override void Compute(Dictionary<int, GameObject> m_boneObjectMap, ref HumanPose human_pose)
    {
        foreach (KeyValuePair<int, GameObject> obj in m_boneObjectMap)
        {
            foreach (SkeletonBone refBone in destination_avatar.humanDescription.skeleton)
            {
                if (refBone.name == obj.Value.name)
                {
                    obj.Value.transform.localPosition = refBone.position;
                    obj.Value.transform.localRotation = refBone.rotation;
                }
            }
        }
    }
}
