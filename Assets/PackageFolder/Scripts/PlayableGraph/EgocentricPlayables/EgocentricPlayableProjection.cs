using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;

public class EgocentricProjectionBehaviour : PlayableBehaviour
{
    private HumanBodyBones m_bone;

    public void Setup(HumanBodyBones bone)
    {
        m_bone = bone;
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        TestEgocentricOutput output = (TestEgocentricOutput)playerData;
        Vector3 previous = output.GetTarget(m_bone);
        Vector3 result = output.Calculate(m_bone);
        output.SetTarget(HumanBodyBones.LeftHand, result);

        //Debug.Log("Egocentric Process Frame | Previous: " + previous + " Result: " + result);
    }
}
