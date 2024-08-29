using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EgocentricIKChainBehaviour : PlayableBehaviour
{


    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        Debug.Log("Behaviour Play info: " + info.output.GetUserData().name);

    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        Debug.Log("Process Frame");
    }

}
