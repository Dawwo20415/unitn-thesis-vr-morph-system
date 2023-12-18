using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarOperationTPose : AvatarOperation
{
    public void Start()
    {
        
    }

    public override void Compute(Dictionary<int, GameObject> m_boneObjectMap, ref HumanPose human_pose)
    {
        Debug.Log("Test", this);
    }
}
