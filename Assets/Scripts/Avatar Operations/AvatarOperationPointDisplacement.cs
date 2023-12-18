using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarOperationPointDisplacement : AvatarOperation
{
    public List<DiscreteDisplacer> displacers = new List<DiscreteDisplacer>(); 

    private void Start()
    {
        
    }

    public override void Compute(Dictionary<int, GameObject> m_boneObjectMap, ref HumanPose human_pose)
    {
        foreach (DiscreteDisplacer disp in displacers)
        {
            disp.computeDisplacement();
        }
    }
}
